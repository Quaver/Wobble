using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlus : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        private WobbleFontStore _font;
        public WobbleFontStore Font
        {
            get => _font;
            set
            {
                if (value == _font)
                    return;

                if (_font != null)
                    _font.Changed -= OnFontChanged;

                _font = value;

                if (_font != null)
                    _font.Changed += OnFontChanged;

                RefreshText();
            }
        }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;

        /// <summary>
        ///     Scale at which the cached line bounds were last calculated.
        /// </summary>
        private float _renderScale;

        /// <summary>
        ///     Applies the shared font baseline offset to uncached text.
        /// </summary>
        private float _verticalDrawOffset;

        /// <summary>
        ///     Height of the font's representative capital glyph.
        /// </summary>
        public float CapHeight { get; private set; }

        /// <summary>
        ///     Distance from the text bounds to the top of the capital glyph area.
        /// </summary>
        public float CapTopOffset { get; private set; }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (value == _fontSize)
                    return;

                _fontSize = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                _text = value ?? "";

                RefreshText();
            }
        }

        /// <summary>
        ///     The tint this QuaverSprite will inherit.
        /// </summary>
        private Color _tint = Color.White;
        public Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;

                Children.ForEach(x =>
                {
                    if (x is Sprite sprite)
                    {
                        sprite.Tint = value;
                    }
                });
            }
        }

        /// <summary>
        ///     The alignment of the text
        /// </summary>
        private TextAlignment _textAlignment = TextAlignment.Left;
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (value == _textAlignment)
                    return;

                _textAlignment = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     The maximal width of the text; the text will be wrapped to fit.
        /// </summary>
        private float? _maxWidth = null;
        public float? MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (value == _maxWidth)
                    return;

                _maxWidth = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     If the text uses caching to a RenderTarget2D rather than drawing as-is.
        ///     Caching is useful for text that does not change often to increase performance and is on by default.
        ///     However, you may want to turn caching off for text that frequently changes (ex. millisecond clocks/timers)
        /// </summary>
        private bool _isCached;
        public bool IsCached
        {
            get => _isCached;
            set
            {
                if (value == _isCached)
                    return;

                _isCached = value;
                RefreshText();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="cache"></param>
        public SpriteTextPlus(WobbleFontStore font, string text, int size = 0, bool cache = true)
        {
            _font = font;
            _font.Changed += OnFontChanged;
            _text = text;
            _isCached = cache;

            _fontSize = size == 0 ? Font.DefaultSize : size;
            _renderScale = SpriteTextPlusLine.GetRenderScale();
            SetChildrenAlpha = true;

            RefreshText();

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.SpriteTextPlusDebugRegistry.Register(this);
#endif
        }

        public override void Update(GameTime gameTime)
        {
            if (IsCached)
            {
                var renderScale = SpriteTextPlusLine.GetRenderScale();

                if (Math.Abs(_renderScale - renderScale) > float.Epsilon)
                {
                    _renderScale = renderScale;
                    RefreshText();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void RefreshText()
        {
#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusRefresh();
#endif

            for (var i = Children.Count - 1; i >= 0; i--)
                Children[i].Destroy();

            // TODO: Actually make this work to set the width/height.
            if (!IsCached)
            {
                SetSize();
                return;
            }

            float width = 0, height = 0;
            var lineSprites = new List<SpriteTextPlusLine>();

            var lines = Text?.Split('\n').ToList() ?? new List<string>();
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                var lineSprite = new SpriteTextPlusLine(Font, line, FontSize);

                // Empty lines are valid (for example, consecutive newlines). Some font/render-scale
                // combinations give their empty sprite a small non-zero width, but there is nothing
                // that can be wrapped in that case.
                if (line.Length > 0 && MaxWidth != null && lineSprite.LayoutWidth > MaxWidth)
                {
                    // Try to split the line on spaces to fit it into MaxWidth.
                    var spaces = new List<int>();
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (char.IsWhiteSpace(line[i]))
                            spaces.Add(i);
                    }

                    // Assumes wider substrings are produced by appending more characters. That matches the current
                    // supported text path and avoids measuring every possible line cut while wrapping.
                    var splitOnIndex = FindLastFittingIndex(spaces, line);

                    // It's always initialized, but the C# compiler isn't smart enough to figure that out.
                    int? nextLineStart = null;

                    if (splitOnIndex == -1)
                    {
                        // Splitting even on the first whitespace gives a line that's too long (or there are no spaces at all),
                        // so split on any character.
                        //
                        // Splitting on arbitrary characters like this is questionable, because while even in English
                        // splitting mid-word can produce nonsensical results, in some complex scripts it doesn't
                        // make any sense whatsoever to split on something that's not a space. But, once again,
                        // we don't support complex scripts yet, and this is a decent enough fallback to make sure
                        // the lines don't get too off max width.
                        var lastIndex = line.Length;
                        if (spaces.Count > 0)
                            lastIndex = spaces[0];

                        nextLineStart = FindLastFittingCharacterIndex(line, lastIndex);
                        var lineCut = line.Substring(0, nextLineStart.Value);
                        lineSprite.Destroy();
                        lineSprite = new SpriteTextPlusLine(Font, lineCut, FontSize);
                    }
                    else
                    {
                        var lineBeforeSpace = line.Substring(0, spaces[splitOnIndex]);
                        lineSprite.Destroy();
                        lineSprite = new SpriteTextPlusLine(Font, lineBeforeSpace, FontSize);
                        nextLineStart = spaces[splitOnIndex] + 1; // Skip over the space that we replaced.
                    }

                    // Insert the remaining part of the line into the list to be iterated over next.
                    Debug.Assert(nextLineStart != null);
                    // Do not append an empty remainder after consuming the final character. Besides
                    // adding a phantom line, very narrow MaxWidth values could then try to wrap that
                    // empty line again on the next iteration.
                    if (nextLineStart.Value < line.Length)
                    {
                        var lineAfterSpace = line.Substring(nextLineStart.Value);
                        lines.Insert(lineIndex + 1, lineAfterSpace);
                    }
                }

                lineSprite.Parent = this;
                lineSprite.Y = height + lineSprite.VerticalLayoutOffset;
                lineSprite.UsePreviousSpriteBatchOptions = true;
                lineSprite.Tint = Tint;
                lineSprite.Alpha = Alpha;
                lineSprites.Add(lineSprite);

                if (lineSprites.Count == 1)
                {
                    CapHeight = lineSprite.CapHeight;
                    CapTopOffset = lineSprite.CapTopOffset;
                }

                width = Math.Max(width, lineSprite.LayoutWidth);

                height += lineSprite.LayoutHeight;
            }

            Size = new ScalableVector2(width, height);

            foreach (var lineSprite in lineSprites)
            {
                lineSprite.Alignment = Alignment.TopLeft;
                lineSprite.X = GetLineX(width, lineSprite.LayoutWidth);
            }
        }

        private void OnFontChanged(object sender, EventArgs e) => RefreshText();

        private int FindLastFittingIndex(IReadOnlyList<int> indexes, string line)
        {
            var result = -1;
            var lo = 0;
            var hi = indexes.Count - 1;

            while (lo <= hi)
            {
                var mid = lo + (hi - lo) / 2;
                var index = indexes[mid];

                if (MeasureLineWidth(line.Substring(0, index)) <= MaxWidth)
                {
                    result = mid;
                    lo = mid + 1;
                }
                else
                    hi = mid - 1;
            }

            return result;
        }

        private int FindLastFittingCharacterIndex(string line, int lastIndex)
        {
            var result = 1;
            var lo = 1;
            var hi = lastIndex;

            while (lo <= hi)
            {
                var mid = lo + (hi - lo) / 2;

                // If we're left with 1 character, just go with it even if we're over MaxWidth.
                if (mid == 1 || MeasureLineWidth(line.Substring(0, mid)) <= MaxWidth)
                {
                    result = mid;
                    lo = mid + 1;
                }
                else
                    hi = mid - 1;
            }

            return result;
        }

        private float MeasureLineWidth(string line)
        {
            var scale = SpriteTextPlusLine.GetRenderScale();
            Font.FontSize = FontSize * scale;
            return (float) Math.Ceiling(Font.Store.MeasureString(line).X) / scale;
        }

        /// <summary>
        ///     Truncates the text with an elipsis according to <see cref="maxWidth"/>
        /// </summary>
        /// <param name="maxWidth"></param>
        public void TruncateWithEllipsis(int maxWidth)
        {
            var originalText = Text;

            // Multi-line (MaxWidth) + Ellipis truncation
            if (Children.Count > 1 && Children.All(x => x is SpriteTextPlusLine))
            {
                var text = Text;

                Font.FontSize = FontSize;
                var totalWidth = Font.Store.MeasureString(text).X;

                while (totalWidth > maxWidth)
                {
                    text = text.Substring(0, text.Length - 1);

                    Font.FontSize = FontSize;
                    totalWidth = Font.Store.MeasureString(text).X;
                }

                Text = text;
            }
            // Single line truncation
            else
            {
                while (Width > maxWidth)
                    Text = Text.Substring(0, Text.Length - 1);
            }

            if (Text != originalText)
                Text += "...";
        }

        public override void DrawToSpriteBatch()
        {
            if (IsCached || !Visible)
                return;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(false);
#endif

            SetSize();
            var drawPosition = AbsolutePosition;
            drawPosition.Y += _verticalDrawOffset * AbsoluteScale.Y;
            Font.Store.DrawText(GameBase.Game.SpriteBatch, Text, drawPosition, _tint * Alpha, scale: AbsoluteScale);
        }

        public override void Destroy()
        {
            if (_font != null)
                _font.Changed -= OnFontChanged;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.SpriteTextPlusDebugRegistry.Unregister(this);
#endif

            base.Destroy();
        }

        private void SetSize()
        {
            Font.FontSize = FontSize;
            var (x, y) = Font.Store.MeasureString(Text);
            SpriteTextPlusLineRaw.GetVerticalLayout(Font, out var layoutHeight, out _verticalDrawOffset,
                out var capHeight);
            CapHeight = capHeight;
            CapTopOffset = (layoutHeight - capHeight) / 2f;
            Size = new ScalableVector2(x, Math.Max(y, layoutHeight));
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private float GetLineX(float availableWidth, float lineWidth)
        {
            switch (TextAlignment)
            {
                case TextAlignment.Left:
                    return 0;
                case TextAlignment.Center:
                    return (availableWidth - lineWidth) / 2f;
                case TextAlignment.Right:
                    return availableWidth - lineWidth;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
