using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI.Debugging
{
    public class FpsCounter : Container
    {
        /// <summary>
        ///     The current frame rate.
        /// </summary>
        public int FrameRate { get; private set; }

        /// <summary>
        ///     The amount of frames that we currently have.
        /// </summary>
        private int FrameCounter { get; set; }

        /// <summary>
        ///     The SpriteText that displays the FPS value.
        /// </summary>
        public SpriteTextBitmap TextFps { get; }

        /// <summary>
        ///     The current update rate.
        /// </summary>
        public int UpdateRate { get; private set; }

        /// <summary>
        ///     The amount of updates that we currently have.
        /// </summary>
        private int UpdateCounter { get; set; }

        /// <summary>
        ///     The SpriteText that displays the UPS value.
        /// </summary>
        public SpriteTextBitmap TextUps { get; }

        /// <summary>
        ///     The amount of time elapsed so we can begin counting each second.
        /// </summary>
        private TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        public FpsCounter(BitmapFont font, int size)
        {
            TextFps = new SpriteTextBitmap(font, "0 FPS", false)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                FontSize = size
            };

            TextUps = new SpriteTextBitmap(font, "0 UPS", false)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                FontSize = size,
                Y = TextFps.Size.Y.Value
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime;

            if (ElapsedTime > TimeSpan.FromSeconds(1))
            {
                ElapsedTime -= TimeSpan.FromSeconds(1);

                var oldFrameRate = FrameRate;
                FrameRate = FrameCounter;
                FrameCounter = 0;
                if (oldFrameRate != FrameRate)
                    TextFps.Text = $"{FrameRate} FPS";

                var oldUpdateRate = UpdateRate;
                UpdateRate = UpdateCounter;
                UpdateCounter = 0;
                if (oldUpdateRate != UpdateRate)
                    TextUps.Text = $"{UpdateRate} UPS";

                TextUps.Y = TextFps.Y + TextFps.Size.Y.Value;
                Size = new ScalableVector2(
                    Math.Max(TextFps.Size.X.Value, TextUps.Size.X.Value),
                    TextFps.Size.Y.Value + TextUps.Size.Y.Value
                );
            }

            // The frame counter updates after the text is refreshed (since Draw happens after Update),
            // so update the update counter after the text is refreshed, too.
            UpdateCounter++;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            FrameCounter++;
            base.Draw(gameTime);
        }
    }
}