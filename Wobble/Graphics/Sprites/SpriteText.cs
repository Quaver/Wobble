using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics.BitmapFonts;
using Wobble.Logging;
using Wobble.Window;

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
        ///     The amount of text updates that have occurred in the previous second.
        /// </summary>
        private int AmountOfTextUpdatesInSecond { get; set; }

        /// <summary>
        ///     The amount of time that has elapsed since the previous second.
        /// </summary>
        private double TimeSinceLastSecond { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="maxWidth">The maximum width before it wraps to the next line</param>
        /// <exception cref="T:System.ArgumentException"></exception>
        public SpriteText(string font, string text, int fontSize, int maxWidth = int.MaxValue)
        {
            if (string.IsNullOrEmpty(font))
                throw new ArgumentException("Font must be not null or empty.");

            _font = font;
            _text = text;
            _fontSize = fontSize;
            _maxWidth = maxWidth;

            LoadTexture();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            MonitorPerformance(gameTime);
            base.Update(gameTime);
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
                Size = new ScalableVector2(0, 0);
                oldTexture?.Dispose();
                return;
            }

            var scale = WindowManager.ScreenScale.X;

            // Some stuff (namely DrawableLog and the FPS counter) wants to draw text before anything is initialized.
            if (scale == 0)
                scale = 1;

            Image = BitmapFontFactory.Create(Font, Text, FontSize * scale, Color.White, TextAlignment, (int) (MaxWidth * scale));
            Size = new ScalableVector2(Image.Width / scale, Image.Height / scale);

            AmountOfTextUpdatesInSecond++;

            if (AmountOfTextUpdatesInSecond >= 100)
                Logger.Warning($"Danger! Way too many text updates ({AmountOfTextUpdatesInSecond}) happening per second for {Text}",
                    LogType.Runtime, false);

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

        /// <summary>
        ///     Keeps track of how many times the text on this object has been changed.
        ///     This can be a major performance hit, so it's just good to track.
        /// </summary>
        /// <param name="gameTime"></param>
        private void MonitorPerformance(GameTime gameTime)
        {
            TimeSinceLastSecond += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLastSecond < 1000)
                return;

            TimeSinceLastSecond = 0;
            AmountOfTextUpdatesInSecond = 0;
        }
    }
}