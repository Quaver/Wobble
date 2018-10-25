using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics.BitmapFonts;

namespace Wobble.Graphics.Sprites
{
    public class SpriteText : Sprite
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
        private Alignment _textAlignment = Alignment.MidLeft;
        public Alignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                LoadTexture();
            }
        }

        /// <summary>
        ///     If set to true, it will draw at the font size given instead of drawing higher and
        ///     scaling down.
        /// </summary>
        private bool _forceDrawAtSize;
        public bool ForceDrawAtSize
        {
            get => _forceDrawAtSize;
            set
            {
                _forceDrawAtSize = value;
                LoadTexture();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="forceDrawAtSize"></param>
        /// <param name="maxWidth">The maximum width before it wraps to the next line</param>
        /// <exception cref="T:System.ArgumentException"></exception>
        public SpriteText(string font, string text, int fontSize, bool forceDrawAtSize = true, int maxWidth = int.MaxValue)
        {
            if (string.IsNullOrEmpty(font))
                throw new ArgumentException("Font must be not null or empty.");

            _font = font;
            _text = text;
            _fontSize = fontSize;
            _maxWidth = maxWidth;
            _forceDrawAtSize = forceDrawAtSize;

            LoadTexture();
        }

        /// <summary>
        ///     Sets the texture of t
        /// </summary>
        private void LoadTexture()
        {
            var oldTexture = Image;

            if (string.IsNullOrEmpty(Text))
            {
                Image = new Texture2D(GameBase.Game.GraphicsDevice, 1, 1);
                oldTexture?.Dispose();
                return;
            }

            Image = BitmapFontFactory.Create(Font, Text, ForceDrawAtSize ? FontSize : 36, Color.White, TextAlignment, MaxWidth);

            var ratio = ForceDrawAtSize ? 1 : FontSize / 36f;
            Size = new ScalableVector2(Image.Width * ratio, Image.Height * ratio);

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


        /// <summary>
        ///     Fades the sprite to a given color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        public override void FadeToColor(Color color, double dt, float scale)
        {
            var r = MathHelper.Lerp(Tint.R, color.R, (float) Math.Min(dt / scale, 1));
            var g = MathHelper.Lerp(Tint.G, color.G, (float) Math.Min(dt / scale, 1));
            var b = MathHelper.Lerp(Tint.B, color.B, (float) Math.Min(dt / scale, 1));

            Tint = new Color((int)r, (int)g, (int)b);
        }
    }
}