using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI.Debugging
{
    public class FpsCounter : Sprite
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
        ///     The amount of time elapsed so we can begin counting each second.
        /// </summary>
        private TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        ///     The SpriteText that displays the FPS value.
        /// </summary>
        public SpriteTextBitmap TextFps { get; }

        /// <summary>
        ///     The last FPS recorded, so we know if to update the counter.
        /// </summary>
        private int LastRecordedFps { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        public FpsCounter(BitmapFont font, int size) => TextFps = new SpriteTextBitmap(font, "0 FPS", false)
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            FontSize = size
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime;

            if (ElapsedTime <= TimeSpan.FromSeconds(1))
            {
                base.Update(gameTime);
                return;
            }

            ElapsedTime -= TimeSpan.FromSeconds(1);

            var oldFrameRate = FrameRate;
            FrameRate = FrameCounter;
            FrameCounter = 0;

            if (oldFrameRate != FrameRate)
            {
                TextFps.Text = $"{FrameRate} FPS";
                Size = TextFps.Size;
            }

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