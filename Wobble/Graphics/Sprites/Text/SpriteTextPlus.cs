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

        /// <summary>
        ///     Whether this drawable should refresh itself when the font store changes.
        /// </summary>
        private readonly bool _subscribesToFontChanges;

        public virtual WobbleFontStore Font
        {
            get => _font;
            set
            {
                if (value == _font)
                    return;

                if (_font != null && _subscribesToFontChanges)
                    _font.Changed -= OnFontChanged;

                _font = value;

                if (_font != null && _subscribesToFontChanges)
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
        ///     Applies the shared vertical draw-origin offset to uncached text.
        /// </summary>
        private float _verticalDrawOffset;

        /// <summary>
        ///     Applies the shared vertical draw-origin offset to uncached text.
        /// </summary>
        protected float VerticalDrawOffset => _verticalDrawOffset;

        /// <summary>
        ///     Whether construction has completed and derived text-layout hooks can safely run.
        /// </summary>
        private bool IsInitialized { get; set; }

        /// <summary>
        ///     Height of the font's representative capital glyph.
        /// </summary>
        public float CapHeight { get; protected set; }

        /// <summary>
        ///     Distance from the text bounds to the top of the capital glyph area.
        /// </summary>
        public float CapTopOffset { get; protected set; }

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
        public virtual string Text
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
                RefreshText(true);
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
        /// <param name="subscribeToFontChanges"></param>
        public SpriteTextPlus(WobbleFontStore font, string text, int size = 0, bool cache = true, bool subscribeToFontChanges = true)
        {
            _subscribesToFontChanges = subscribeToFontChanges;
            _font = font;

            if (_subscribesToFontChanges)
                _font.Changed += OnFontChanged;

            _text = text;
            _isCached = cache;

            _fontSize = size == 0 ? Font.DefaultSize : size;
            _renderScale = SpriteTextPlusLine.GetRenderScale();
            SetChildrenAlpha = true;

            RefreshText();
            IsInitialized = true;

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
        protected void RefreshText(bool reuseUnchangedLines = false)
        {
#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusRefresh();
#endif

            // TODO: Actually make this work to set the width/height.
            if (!IsCached)
            {
                for (var i = Children.Count - 1; i >= 0; i--)
                    Children[i].Destroy();

                SetSize();

                if (IsInitialized)
                    OnTextLayoutRefreshed();

                return;
            }

            var lines = BuildWrappedLines();
            if (reuseUnchangedLines && LinesMatch(lines))
            {
                if (IsInitialized)
                    OnTextLayoutRefreshed();

                return;
            }

            for (var i = Children.Count - 1; i >= 0; i--)
                Children[i].Destroy();

            float width = 0, height = 0;
            var lineSprites = new List<SpriteTextPlusLine>(lines.Count);
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                var lineSprite = new SpriteTextPlusLine(Font, line, FontSize);

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

            if (IsInitialized)
                OnTextLayoutRefreshed();
        }

        /// <summary>
        ///     Called after the text's cached line layout has been recreated.
        /// </summary>
        protected virtual void OnTextLayoutRefreshed()
        {
        }

        private List<string> BuildWrappedLines() => BuildWrappedLayout().Select(x => x.Text).ToList();

        internal List<WrappedTextLine> BuildWrappedLayout() => BuildWrappedLayout(Text);

        internal List<WrappedTextLine> BuildWrappedLayout(string text) =>
            WrappedTextLayout.Build(text, MaxWidth, MeasureLineWidth);

        internal float MeasureLayoutLineWidth(string text) => MeasureLineWidth(text);

        private bool LinesMatch(IReadOnlyList<string> lines)
        {
            if (Children.Count != lines.Count)
                return false;

            for (var i = 0; i < lines.Count; i++)
            {
                if (!(Children[i] is SpriteTextPlusLine lineSprite) || lineSprite.Text != lines[i])
                    return false;
            }

            return true;
        }

        private void OnFontChanged(object sender, EventArgs e) => RefreshText();

        private float MeasureLineWidth(string line)
        {
            var scale = SpriteTextPlusLine.GetRenderScale();
            Font.FontSize = FontSize * scale;
            return (float)Math.Ceiling(Font.Store.MeasureString(line).X) / scale;
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
            if (_font != null && _subscribesToFontChanges)
                _font.Changed -= OnFontChanged;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.SpriteTextPlusDebugRegistry.Unregister(this);
#endif

            base.Destroy();
        }

        protected void SetSize()
        {
            Font.FontSize = FontSize;
            var (x, y) = Font.Store.MeasureString(Text);
            SpriteTextPlusLineRaw.GetVerticalLayout(Font, out var layoutHeight, out _verticalDrawOffset, out var capHeight);
            CapHeight = capHeight;
            CapTopOffset = (layoutHeight - capHeight) / 2f;
            Size = new ScalableVector2(x, Math.Max(y, layoutHeight));
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected float GetLineX(float availableWidth, float lineWidth)
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