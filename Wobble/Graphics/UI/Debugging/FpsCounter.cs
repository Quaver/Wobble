using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI.Debugging
{
    public class FpsCounter : Sprite
    {
        /// <summary>
        ///     The current frame rate.
        /// </summary>
        private int FrameRate { get; set; }

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
        public SpriteText TextFps { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        public FpsCounter(SpriteFont font, float textScale) => TextFps = new SpriteText
        {
            Font = font,
            Text = "FPS: ",
            Parent = this,
            TextScale = textScale,
            Alignment = Alignment.MidCenter
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime;

            if (ElapsedTime <= TimeSpan.FromSeconds(1))
                return;

            ElapsedTime -= TimeSpan.FromSeconds(1);
            FrameRate = FrameCounter;
            FrameCounter = 0;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            FrameCounter++;
            TextFps.Text = $"FPS: {FrameRate}";

            base.Draw(gameTime);
        }
    }
}