using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.BitmapFonts
{
    public class SpriteTextBitmap : Sprite
    {
        /// <summary>
        ///     The name of the font to use.
        ///
        ///     This will first try to load the font from BitmapFontFactory.CustomFonts.
        ///     If it cannot find it, it will try and load the font from the system.
        /// </summary>
        public string _font;
        public string Font
        {
            get => _font;
            set
            {
                _font = value;
                LoadTexture();
            }
        }

        /// <summary>
        ///     The displayed text.
        /// </summary>
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                LoadTexture();
            }
        }

        /// <summary>
        ///     The size of the font.
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                LoadTexture();
            }
        }

        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                LoadTexture();
            }
        }

        /// <summary>
        ///     The max width of the text.
        /// </summary>
        private int _maxWidth;
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                LoadTexture();
            }
        }

        /// <summary>
        ///     The alignment of the text
        /// </summary>
        private Alignment _textAlignment;
        public Alignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                LoadTexture();
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Sprite text from a bitmap font.
        /// </summary>
        public SpriteTextBitmap(string font, string text, int fontSize, Color color, Alignment textAlignment, int maxWidth)
        {
            if (string.IsNullOrEmpty(font))
                throw new ArgumentException("Font must be not null or empty.");

            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text must not be null or empty.");

            _font = font;
            _text = text;
            _fontSize = fontSize;
            _color = color;
            _maxWidth = maxWidth;
            _textAlignment = textAlignment;

            LoadTexture();
        }

        /// <summary>
        ///     Sets the texture of t
        /// </summary>
        private void LoadTexture()
        {
            var oldTexture = Image;

            Image = BitmapFontFactory.Create(Font, Text, FontSize, Color, TextAlignment, MaxWidth);
            Size = new ScalableVector2(Image.Width, Image.Height);

            oldTexture?.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Image.Dispose();
            base.Destroy();
        }
    }
}