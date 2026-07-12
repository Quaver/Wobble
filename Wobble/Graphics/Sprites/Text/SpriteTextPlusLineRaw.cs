using System;
using Microsoft.Xna.Framework;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLineRaw : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private float _fontSize;
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                RefreshSize();
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
                _text = value;
                RefreshSize();
            }
        }

        /// <summary>
        ///     The measured glyph width before render-target padding is applied.
        /// </summary>
        public float MeasuredWidth { get; private set; }

        /// <summary>
        ///     Content-independent line height used for layout.
        /// </summary>
        internal float LayoutHeight { get; private set; }

        /// <summary>
        ///     Height of a representative capital glyph.
        /// </summary>
        internal float CapHeight { get; private set; }

        /// <summary>
        ///     Padding around the rendered glyphs to prevent texture clipping.
        /// </summary>
        internal float RenderPadding { get; private set; }

        /// <summary>
        ///     Applies a font-size-specific offset while preserving a shared baseline.
        /// </summary>
        internal float VerticalDrawOffset { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0)
        {
            Font = font;
            Text = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            Font.FontSize = FontSize;
            Font.Store.DrawText(GameBase.Game.SpriteBatch, Text, AbsolutePosition, _color, scale: AbsoluteScale);
        }

        private void RefreshSize()
        {
            Font.FontSize = FontSize;

            var (x, y) = Font.Store.MeasureString(Text);
            MeasuredWidth = x;
            RenderPadding = Math.Max(2f, FontSize * 0.25f);
            GetVerticalLayout(Font, out var fontLayoutHeight, out var drawOffset, out var capHeight);
            LayoutHeight = Math.Max(y, fontLayoutHeight);
            CapHeight = capHeight;
            VerticalDrawOffset = drawOffset;

            Y = RenderPadding / 2f + VerticalDrawOffset;
            Size = new ScalableVector2(x + RenderPadding, LayoutHeight + RenderPadding);
        }

        internal static void GetVerticalLayout(WobbleFontStore font, out float layoutHeight,
            out float drawOffset, out float capHeight)
        {
            // "H" measures cap height; "Hgj" adds descenders so every string shares centered bounds and a stable baseline.
            var capBounds = font.Store.TextBounds("H", Vector2.Zero);
            capHeight = capBounds.Y2 - capBounds.Y;

            if (capHeight <= 0)
            {
                layoutHeight = font.Store.LineHeight;
                drawOffset = 0;
                capHeight = layoutHeight;
                return;
            }

            var fullBounds = font.Store.TextBounds("Hgj", Vector2.Zero);
            var fullHeight = fullBounds.Y2 - fullBounds.Y;
            var extensionHeight = Math.Max(0, fullHeight - capHeight);

            layoutHeight = Math.Max(font.Store.LineHeight, capHeight + extensionHeight * 2f);
            drawOffset = (layoutHeight - capHeight) / 2f - capBounds.Y;
        }
    }
}
