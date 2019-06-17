using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlus : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                DisplayedText = WrapText(Text);
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
                DisplayedText = WrapText(value);
            }
        }

        /// <summary>
        ///     The text that'll be displayed on-screen (after wrapping)
        /// </summary>
        public string DisplayedText { get; private set; }

        /// <summary>
        ///     The maximum width of the text.
        /// </summary>
        private int _maxWidth = int.MaxValue;
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                DisplayedText = WrapText(Text);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlus(WobbleFontStore font, string text, int size = 0)
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

            Font.Store.Size = FontSize;
            GameBase.Game.SpriteBatch.DrawString(Font.Store, Text, AbsolutePosition, _color);
        }

        /// <summary>
        ///     <see cref="SpriteTextBitmap"/> OR <see cref="SpriteText"/> for wrapping examples
        /// </summary>
        /// <param name="value"></param>
        private string WrapText(string value)
        {
            Font.Store.Size = FontSize;

            var (x, y) = Font.Store.MeasureString(value);
            Size = new ScalableVector2(x, y);

            return value;
        }
    }
}