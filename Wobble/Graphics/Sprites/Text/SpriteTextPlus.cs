using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Managers;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlus : Sprite
    {
        private RenderTarget2D _blockRenderTarget;
        private bool _cacheDirty = true;
        private bool _cacheBuildScheduled;
        private Vector2 _displayScale;
        private Vector2 _cacheScale;

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

                _font = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;
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
        public override Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;
                base.Tint = value;

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
        ///     Minimum final on-screen glyph size in physical pixels. Set to zero to disable.
        /// </summary>
        private float _minimumPhysicalFontSize;
        public float MinimumPhysicalFontSize
        {
            get => _minimumPhysicalFontSize;
            set
            {
                value = Math.Max(0, value);
                if (value == _minimumPhysicalFontSize)
                    return;
                _minimumPhysicalFontSize = value;
                RefreshText();
            }
        }

        private Vector2 _shadowOffset;
        public Vector2 ShadowOffset
        {
            get => _shadowOffset;
            set
            {
                if (value == _shadowOffset)
                    return;
                _shadowOffset = value;
                RefreshText();
            }
        }

        private Color _shadowColor = Color.Transparent;
        public Color ShadowColor
        {
            get => _shadowColor;
            set
            {
                if (value == _shadowColor)
                    return;
                _shadowColor = value;
                MarkCacheDirty();
            }
        }

        private float _outlineThickness;
        public float OutlineThickness
        {
            get => _outlineThickness;
            set
            {
                value = Math.Max(0, value);
                if (value == _outlineThickness)
                    return;
                _outlineThickness = value;
                RefreshText();
            }
        }

        private Color _outlineColor = Color.Transparent;
        public Color OutlineColor
        {
            get => _outlineColor;
            set
            {
                if (value == _outlineColor)
                    return;
                _outlineColor = value;
                MarkCacheDirty();
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
            _text = text;
            _isCached = cache;

            _fontSize = size == 0 ? Font.DefaultSize : size;
            _displayScale = TextRenderQuality.DisplayScale;
            _cacheScale = TextRenderQuality.CacheScale;
            SetChildrenAlpha = true;

            RefreshText();

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.SpriteTextPlusDebugRegistry.Register(this);
#endif
        }

        /// <summary>
        ///     Creates text using a font cached in <see cref="FontManager"/>.
        /// </summary>
        public SpriteTextPlus(string font, string text, int size = 0, bool cache = true)
            : this(FontManager.GetWobbleFont(font), text, size, cache)
        {
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

            MarkCacheDirty();

            float width = 0, height = 0;
            var effectPadding = GetEffectPadding();
            var renderFont = GetRenderFont();
            var renderFontSize = GetRenderFontSize();

            var lines = Text?.Split('\n').ToList() ?? new List<string>();
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                var lineSprite = new SpriteTextPlusLine(renderFont, line, renderFontSize, false);

                if (MaxWidth != null && lineSprite.Width > MaxWidth)
                {
                    Debug.Assert(line.Length > 0);

                    // Try to split the line on spaces to fit it into MaxWidth.
                    var spaces = new List<int>();
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (char.IsWhiteSpace(line[i]))
                            spaces.Add(i);
                    }

                    // Binary search would be great for the next two (as long as we're assuming that
                    // more characters == longer lines, which will not hold for complex scripts,
                    // which aren't supported yet anyway), but C# doesn't have a built-in method
                    // for binary search by an arbitrary predicate. So I guess we'll just go with a regular find-last,
                    // which can be slower, but has a bonus of not making any of the aforementioned assumptions.
                    var splitOnIndex = spaces.FindLastIndex(spacePosition =>
                    {
                        var lineBeforeSpace = line.Substring(0, spacePosition);
                        var sprite = new SpriteTextPlusLine(renderFont, lineBeforeSpace, renderFontSize, false);
                        var lineWidth = sprite.Width;
                        sprite.Destroy();
                        return lineWidth <= MaxWidth;
                    });

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

                        for (var i = lastIndex; i != 0; i--)
                        {
                            var lineCut = line.Substring(0, i);
                            var sprite = new SpriteTextPlusLine(renderFont, lineCut, renderFontSize, false);

                            // If we're left with 1 character, just go with it even if we're over MaxWidth.
                            if (sprite.Width > MaxWidth && i > 1)
                            {
                                sprite.Destroy();
                                continue;
                            }

                            lineSprite.Destroy();
                            lineSprite = sprite;
                            nextLineStart = i;
                            break;
                        }
                    }
                    else
                    {
                        var lineBeforeSpace = line.Substring(0, spaces[splitOnIndex]);
                        lineSprite.Destroy();
                        lineSprite = new SpriteTextPlusLine(renderFont, lineBeforeSpace, renderFontSize, false);
                        nextLineStart = spaces[splitOnIndex] + 1; // Skip over the space that we replaced.
                    }

                    // Insert the remaining part of the line into the list to be iterated over next.
                    Debug.Assert(nextLineStart != null);
                    var lineAfterSpace = line.Substring(nextLineStart.Value);
                    lines.Insert(lineIndex + 1, lineAfterSpace);
                }

                lineSprite.Parent = this;
                lineSprite.Alignment = ConvertTextAlignment();
                if (TextAlignment == TextAlignment.Left)
                    lineSprite.X = effectPadding;
                else if (TextAlignment == TextAlignment.Right)
                    lineSprite.X = -effectPadding;
                lineSprite.Y = effectPadding + height;
                lineSprite.UsePreviousSpriteBatchOptions = true;
                lineSprite.Tint = Tint;
                lineSprite.Alpha = Alpha;

                width = Math.Max(width, lineSprite.Width);

                renderFont.FontSize = renderFontSize;
                height += renderFont.Store.LineHeight;
            }

            Size = new ScalableVector2(width + effectPadding * 2, height + effectPadding * 2);
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

        public override void Update(GameTime gameTime)
        {
            var displayScale = TextRenderQuality.DisplayScale;
            var cacheScale = TextRenderQuality.CacheScale;

            if (Vector2.DistanceSquared(displayScale, _displayScale) > 0.000001f ||
                Vector2.DistanceSquared(cacheScale, _cacheScale) > 0.000001f)
            {
                _displayScale = displayScale;
                _cacheScale = cacheScale;
                RefreshText();
            }

            if (IsCached && _cacheDirty && !_cacheBuildScheduled)
            {
                _cacheBuildScheduled = true;
                GameBase.Game.ScheduledRenderTargetDraws.Add(CacheBlock);
            }

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            if (IsCached)
            {
                if (_blockRenderTarget != null && !_blockRenderTarget.IsDisposed)
                {
#if DEBUG
                    global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(true);
#endif
                    base.DrawToSpriteBatch();
                }
                return;
            }

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(false);
#endif

            foreach (var line in Children.OfType<SpriteTextPlusLine>())
            {
                var display = TextRenderQuality.DisplayScale;
                var fontScale = display.Y;
                var position = new Vector2(
                    TextRenderQuality.Snap(line.AbsolutePosition.X, display.X),
                    TextRenderQuality.Snap(line.AbsolutePosition.Y, display.Y));
                DrawLine(line, position, line.AbsoluteScale / fontScale, fontScale,
                    Vector2.One, _tint * Alpha, Alpha);
            }
        }

        public override void Destroy()
        {
#if DEBUG
            global::Wobble.Graphics.UI.Debugging.SpriteTextPlusDebugRegistry.Unregister(this);
#endif

            if (_blockRenderTarget != null && !_blockRenderTarget.IsDisposed)
                _blockRenderTarget.Dispose();

            _blockRenderTarget = null;
            base.Destroy();
        }

        protected override void OnRectangleRecalculated()
        {
            if (Image == null)
                return;

            if (Rotation != 0)
            {
                base.OnRectangleRecalculated();
                return;
            }

            var displayScale = TextRenderQuality.DisplayScale;
            var x = TextRenderQuality.Snap(ScreenRectangle.X, displayScale.X);
            var y = TextRenderQuality.Snap(ScreenRectangle.Y, displayScale.Y);
            var right = TextRenderQuality.Snap(ScreenRectangle.Right, displayScale.X);
            var bottom = TextRenderQuality.Snap(ScreenRectangle.Bottom, displayScale.Y);
            var width = Math.Max(0, right - x);
            var height = Math.Max(0, bottom - y);

            RenderRectangle = new MonoGame.Extended.RectangleF(
                x + width * Pivot.X,
                y + height * Pivot.Y,
                width,
                height);
        }

        private float GetRenderFontSize()
        {
            var minimumLogicalSize = MinimumPhysicalFontSize <= 0
                ? 0
                : MinimumPhysicalFontSize / Math.Max(_displayScale.X, _displayScale.Y);
            return Math.Max(FontSize, minimumLogicalSize);
        }

        private WobbleFontStore GetRenderFont()
        {
            var physicalSize = GetRenderFontSize() * Math.Max(_displayScale.X, _displayScale.Y);
            return Font.SmallTextAlternative != null && physicalSize < Font.SmallTextThreshold
                ? Font.SmallTextAlternative
                : Font;
        }

        private float GetEffectPadding() => (float)Math.Ceiling(Math.Max(
            OutlineThickness,
            Math.Max(Math.Abs(ShadowOffset.X), Math.Abs(ShadowOffset.Y))));

        private void MarkCacheDirty()
        {
            _cacheDirty = true;
        }

        private void CacheBlock()
        {
            _cacheBuildScheduled = false;
            if (IsDisposed || !IsCached || !_cacheDirty)
                return;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusCacheBuild();
#endif

            _ = GameBase.Game.TryEndBatch();
            var pixelWidth = Math.Max(1, (int)Math.Round(Width * _cacheScale.X));
            var pixelHeight = Math.Max(1, (int)Math.Round(Height * _cacheScale.Y));

            if (pixelWidth <= 0 || pixelHeight <= 0)
            {
                _cacheDirty = false;
                return;
            }

            if (_blockRenderTarget == null || _blockRenderTarget.IsDisposed ||
                _blockRenderTarget.Width != pixelWidth || _blockRenderTarget.Height != pixelHeight)
            {
                if (_blockRenderTarget != null && !_blockRenderTarget.IsDisposed)
                    _blockRenderTarget.Dispose();

                _blockRenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, pixelWidth, pixelHeight, false,
                    GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            }

            GameBase.Game.GraphicsDevice.SetRenderTarget(_blockRenderTarget);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);
            // FontStash can add fallback glyphs to an atlas while DrawText is running. Immediate
            // submission ensures an atlas update cannot invalidate glyphs queued earlier in this
            // one-off cache build (notably CJK and emoji fallback glyphs).
            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, RasterizerState.CullNone);

            var padding = GetEffectPadding();
            foreach (var line in Children.OfType<SpriteTextPlusLine>())
            {
                var x = padding;
                if (TextAlignment == TextAlignment.Center)
                    x += (Width - padding * 2 - line.Width) / 2f;
                else if (TextAlignment == TextAlignment.Right)
                    x += Width - padding * 2 - line.Width;

                var position = new Vector2(x * _cacheScale.X, line.Y * _cacheScale.Y);
                var glyphScale = _cacheScale.Y;
                var drawScale = new Vector2(_cacheScale.X / glyphScale, 1f);
                DrawLine(line, position, drawScale, glyphScale,
                    _cacheScale, Color.White, 1f);
            }

            GameBase.Game.SpriteBatch.End();
            GameBase.Game.GraphicsDevice.SetRenderTarget(null);
            Image = _blockRenderTarget;
            _cacheDirty = false;
        }

        private void DrawLine(SpriteTextPlusLine line, Vector2 position, Vector2 drawScale,
            float fontScale, Vector2 effectOffsetScale, Color foregroundColor, float effectAlpha)
        {
            line.Font.FontSize = line.FontSize * fontScale;
            var spriteBatch = GameBase.Game.SpriteBatch;

            if (ShadowColor.A > 0 && ShadowOffset != Vector2.Zero)
                line.Font.Store.DrawText(spriteBatch, line.Text,
                    position + ShadowOffset * effectOffsetScale, ShadowColor * effectAlpha, scale: drawScale);

            if (OutlineColor.A > 0 && OutlineThickness > 0)
            {
                const int samples = 8;
                for (var i = 0; i < samples; i++)
                {
                    var angle = MathHelper.TwoPi * i / samples;
                    var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                                 OutlineThickness * effectOffsetScale;
                    line.Font.Store.DrawText(spriteBatch, line.Text, position + offset,
                        OutlineColor * effectAlpha, scale: drawScale);
                }
            }

            line.Font.Store.DrawText(spriteBatch, line.Text, position, foregroundColor, scale: drawScale);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Alignment ConvertTextAlignment()
        {
            switch (TextAlignment)
            {
                case TextAlignment.Left:
                    return Alignment.TopLeft;
                case TextAlignment.Center:
                    return Alignment.TopCenter;
                case TextAlignment.Right:
                    return Alignment.TopRight;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
