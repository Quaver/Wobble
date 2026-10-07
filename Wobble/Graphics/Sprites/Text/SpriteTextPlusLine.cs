using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Logging;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLine : Sprite
    {
        /// <summary>
        ///     The underlying text rendering component.
        /// </summary>
        private readonly SpriteTextPlusLineRaw _raw;

        /// <summary>
        ///     Whether the cached texture needs to be refreshed.
        /// </summary>
        private bool _dirty;

        /// <summary>
        ///     Logical font-size ranges before render-scale multiplication.
        /// </summary>
        private readonly List<TextFontSizeRange> _textFontSizeRanges = new List<TextFontSizeRange>();

        /// <summary>
        ///     Reused scaled range buffer passed to the raw renderer.
        /// </summary>
        private readonly List<TextFontSizeRange> _scaledTextFontSizeRanges = new List<TextFontSizeRange>();

        /// <summary>
        ///     Current WindowManager scale.
        /// </summary>
        private float _scale;
        private float Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                    return;

                // Retrieve the original font size (computed with old scale).
                var fontSize = FontSize;

                _scale = value;

                // Set the font size with the new scale.
                FontSize = fontSize;
            }
        }

        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get => _raw.Font; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        public float FontSize
        {
            get => _raw.FontSize / _scale;
            set
            {
                _raw.FontSize = value * _scale;
                ApplyTextFontSizeRanges();
            }
        }

        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        public string Text
        {
            get => _raw.Text;
            set
            {
                _raw.Text = value;
                SetSize();
                _dirty = true;
            }
        }

        /// <summary>
        ///     The width of the rendered glyphs, excluding the trailing render-target padding.
        /// </summary>
        public float LayoutWidth { get; private set; }

        /// <summary>
        ///     Content-independent height of the logical text line.
        /// </summary>
        public float LayoutHeight => _raw.LayoutHeight / _scale;

        /// <summary>
        ///     Height of a representative capital glyph at the logical render scale.
        /// </summary>
        public float CapHeight => _raw.CapHeight / _scale;

        /// <summary>
        ///     Distance from the logical bounds to the top of the capital glyph area.
        /// </summary>
        public float CapTopOffset => _raw.CapTopOffset / _scale;

        /// <summary>
        ///     Offsets the render-target padding so it does not affect visual alignment.
        /// </summary>
        internal float VerticalLayoutOffset => -_raw.RenderPadding / (2f * _scale);

        /// <summary>
        ///     The rendertarget used to cache the text
        /// </summary>
        private RenderTarget2D RenderTarget { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="boldFont"></param>
        public SpriteTextPlusLine(WobbleFontStore font, string text, float size = 0,
            WobbleFontStore boldFont = null, WobbleFontStore italicFont = null,
            WobbleFontStore boldItalicFont = null)
        {
            _scale = GetRenderScale();

            _raw = new SpriteTextPlusLineRaw(font, text, size * _scale, boldFont, italicFont,
                boldItalicFont)
            {
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    DoNotScale = true,
                    BlendState = BlendState.AlphaBlend
                }
            };

            SetSize();

            Image = WobbleAssets.WhiteBox;
            Visible = false;
            _dirty = true;
        }

        /// <summary>
        ///     Get the current WindowManager scale and check that it's valid.
        /// </summary>
        /// <returns></returns>
        internal static float GetRenderScale()
        {
            var scale = Math.Max(WindowManager.ScreenScale.X, WindowManager.ScreenScale.Y);

            // Some stuff (namely DrawableLog and the FPS counter) wants to draw text before anything is initialized.
            if (scale == 0)
                scale = 1;

            return Math.Max(1, scale);
        }

        /// <summary>
        ///     Set the component size taking rounding into account.
        /// </summary>
        private void SetSize()
        {
            LayoutWidth = (float)Math.Ceiling(_raw.MeasuredWidth) / _scale;

            // Round the size the same way it will be rounded during rendering.
            var (width, height) = _raw.AbsoluteSize;
            var pixelWidth = Math.Ceiling(width);
            var pixelHeight = Math.Ceiling(height);

            var flooredSize = new ScalableVector2((float)pixelWidth, (float)pixelHeight);
            Size = flooredSize / _scale;
        }

        /// <summary>
        ///     Applies font sizes to character ranges and refreshes this line's logical bounds.
        /// </summary>
        internal void SetTextFontSizeRanges(IReadOnlyList<TextFontSizeRange> ranges)
        {
            _textFontSizeRanges.Clear();
            _textFontSizeRanges.AddRange(ranges);
            ApplyTextFontSizeRanges();
        }

        /// <summary>
        ///     Clears all custom font sizes from this line.
        /// </summary>
        internal void ClearTextFontSizeRanges()
        {
            if (_textFontSizeRanges.Count == 0)
                return;

            _textFontSizeRanges.Clear();
            ApplyTextFontSizeRanges();
        }

        /// <summary>
        ///     Measures from the beginning of this line to a UTF-16 character index.
        /// </summary>
        internal float MeasureTextWidth(int textIndex) => _raw.MeasureWidthToIndex(textIndex) / _scale;

        /// <summary>
        ///     Reapplies logical size ranges at the current render scale.
        /// </summary>
        private void ApplyTextFontSizeRanges()
        {
            if (_textFontSizeRanges.Count == 0)
                _raw.ClearTextFontSizeRanges();
            else
            {
                _scaledTextFontSizeRanges.Clear();

                for (var i = 0; i < _textFontSizeRanges.Count; i++)
                {
                    var range = _textFontSizeRanges[i];
                    _scaledTextFontSizeRanges.Add(new TextFontSizeRange(range.StartIndex, range.Length, range.FontSize * _scale));
                }

                _raw.SetTextFontSizeRanges(_scaledTextFontSizeRanges);
            }

            SetSize();
            _dirty = true;
        }

        /// <summary>
        ///     Applies bold styling to character ranges.
        /// </summary>
        internal void SetTextBoldRanges(IReadOnlyList<TextBoldRange> ranges)
        {
            _raw.SetTextBoldRanges(ranges);
            SetSize();
            _dirty = true;
        }

        /// <summary>
        ///     Clears all bold styling from this line.
        /// </summary>
        internal void ClearTextBoldRanges()
        {
            if (!_raw.ClearTextBoldRanges())
                return;

            SetSize();
            _dirty = true;
        }

        /// <summary>
        ///     Applies italic styling to character ranges.
        /// </summary>
        internal void SetTextItalicRanges(IReadOnlyList<TextItalicRange> ranges)
        {
            _raw.SetTextItalicRanges(ranges);
            SetSize();
            _dirty = true;
        }

        /// <summary>
        ///     Clears all italic styling from this line.
        /// </summary>
        internal void ClearTextItalicRanges()
        {
            if (!_raw.ClearTextItalicRanges())
                return;

            SetSize();
            _dirty = true;
        }

        /// <summary>
        ///     Applies colors to ranges of characters without splitting the text into separate draw calls.
        /// </summary>
        /// <param name="ranges"></param>
        internal void SetTextColorRanges(IReadOnlyList<TextColorRange> ranges)
        {
            _raw.SetTextColorRanges(ranges);
            _dirty = true;
        }

        /// <summary>
        /// </summary>
        internal void ClearTextColorRanges()
        {
            _raw.ClearTextColorRanges();
            _dirty = true;
        }

        /// <summary>
        ///     Applies underlines to ranges of characters using their effective text colors.
        /// </summary>
        internal void SetTextUnderlineRanges(IReadOnlyList<TextUnderlineRange> ranges)
        {
            _raw.SetTextUnderlineRanges(ranges);
            _dirty = true;
        }

        /// <summary>
        ///     Clears all underlines from this line.
        /// </summary>
        internal void ClearTextUnderlineRanges()
        {
            if (_raw.ClearTextUnderlineRanges())
                _dirty = true;
        }

        /// <summary>
        ///     Builds the per-glyph colors expected by FontStashSharp from UTF-16 character ranges.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="fontSize"></param>
        /// <param name="text"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        internal static Color[] CreateGlyphColors(WobbleFontStore font, float fontSize, string text, IReadOnlyList<TextColorRange> ranges)
        {
            font.FontSize = fontSize;

            var glyphs = font.Store.GetGlyphs(text, Vector2.Zero);
            var colors = new List<Color>(glyphs.Count);
            var hasColoredGlyph = false;

            foreach (var glyph in glyphs)
            {
                // FontStashSharp only consumes a color when it draws a non-empty glyph.
                if (glyph.Bounds.Width == 0 || glyph.Bounds.Height == 0)
                    continue;

                var textIndex = GetTextIndex(text, glyph.Index);
                var color = Color.White;
                var isColored = false;

                for (var i = 0; i < ranges.Count; i++)
                {
                    var range = ranges[i];

                    if (textIndex < range.StartIndex || textIndex >= range.StartIndex + range.Length)
                        continue;

                    color = range.Color;
                    isColored = true;
                }

                colors.Add(color);
                hasColoredGlyph |= isColored;
            }

            return hasColoredGlyph ? colors.ToArray() : null;
        }

        /// <summary>
        ///     Converts FontStashSharp's codepoint index into a UTF-16 string index.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="codepointIndex"></param>
        /// <returns></returns>
        private static int GetTextIndex(string text, int codepointIndex)
        {
            var textIndex = 0;

            for (var i = 0; i < codepointIndex && textIndex < text.Length; i++)
                textIndex += char.IsSurrogatePair(text, textIndex) ? 2 : 1;

            return textIndex;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update the Scale and schedules the component to be rendered into a texture if necessary.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Scale = GetRenderScale();

            if (_dirty)
            {
                _dirty = false;
                GameBase.Game.ScheduleRenderTargetDraw(() => Cache(gameTime));
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (RenderTarget != null && !RenderTarget.IsDisposed)
                RenderTarget.Dispose();

            Image = null;

            base.Destroy();
        }

        public override void DrawToSpriteBatch()
        {
#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(true);
#endif

            base.DrawToSpriteBatch();
        }

        /// <summary>
        ///     Round the position to align with pixels exactly.
        /// </summary>
        protected override void OnRectangleRecalculated()
        {
            // Update the render rectangle.
            var x = ScreenRectangle.X;
            var y = ScreenRectangle.Y;

            if (Rotation == 0)
            {
                // Round the coordinates. Not rounding the coordinates means bad text.
                var pixelX = (int)(x * WindowManager.ScreenScale.X);
                var pixelY = (int)(y * WindowManager.ScreenScale.Y);

                x = pixelX / WindowManager.ScreenScale.X;
                y = pixelY / WindowManager.ScreenScale.Y;
            }

            // Add Width / 2 and Height / 2 to X, Y because that's what Origin is set to (in the Image setter).
            RenderRectangle = new RectangleF(x + ScreenRectangle.Width / 2f, y + ScreenRectangle.Height / 2f, ScreenRectangle.Width, ScreenRectangle.Height);
        }

        /// <summary>
        ///     Render the text into a texture.
        /// </summary>
        /// <param name="gameTime"></param>
        private void Cache(GameTime gameTime)
        {
            if (IsDisposed)
                return;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusCacheBuild();
#endif

            _ = GameBase.Game.TryEndBatch();
            var (width, height) = _raw.AbsoluteSize;
            var pixelWidth = (int)Math.Ceiling(width);
            var pixelHeight = (int)Math.Ceiling(height);

            if (pixelWidth == 0 || pixelHeight == 0)
            {
                Visible = false;
                return;
            }

            var graphicsDevice = GameBase.Game.GraphicsDevice;
            var recreateRenderTarget = RenderTarget == null || RenderTarget.IsDisposed ||
                                       RenderTarget.IsContentLost || RenderTarget.GraphicsDevice != graphicsDevice ||
                                       RenderTarget.Width != pixelWidth || RenderTarget.Height != pixelHeight;

            if (recreateRenderTarget)
            {
                RenderTarget?.Dispose();
                RenderTarget = new RenderTarget2D(graphicsDevice, pixelWidth, pixelHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            }

            var previousScissorRectangle = graphicsDevice.ScissorRectangle;

            try
            {
                graphicsDevice.SetRenderTarget(RenderTarget);
                graphicsDevice.ScissorRectangle = new Rectangle(0, 0, pixelWidth, pixelHeight);
                graphicsDevice.Clear(Color.Transparent);
                _raw.Draw(gameTime);
                _ = GameBase.Game.TryEndBatch();
            }
            finally
            {
                graphicsDevice.SetRenderTarget(null);
                graphicsDevice.ScissorRectangle = previousScissorRectangle;
            }

            if (recreateRenderTarget || Image != RenderTarget)
                Image = RenderTarget;

            Visible = true;
        }
    }
}
