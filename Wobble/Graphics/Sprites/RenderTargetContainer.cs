using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    /// <summary>
    ///     Sprite used to draw things to a different RenderTarget.
    /// </summary>
    public class RenderTargetContainer : Sprite
    {
        /// <summary>
        ///     The main render target to render to.
        /// </summary>
        private RenderTarget2D RenderTarget { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public RenderTargetContainer(ScalableVector2 size)
        {
            Size = size;
            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) Width, (int) Height);
        }

        /// <summary>
        /// </summary>
        public RenderTargetContainer()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) Width, (int) Height);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

            // Draw all of the children
            Children.ForEach(x =>
            {
                x.UsePreviousSpriteBatchOptions = true;
                x.Draw(gameTime);
            });

            // Attempt to end the spritebatch
            _ = GameBase.Game.TryEndBatch();

            // Reset the render target.
            GameBase.Game.GraphicsDevice.SetRenderTarget(null);

            // Grab the old image so we can set it back later.
            var oldImage = Image;

            // Change the image to the render target
            // and draw it.
            Image = RenderTarget;
            base.Draw(gameTime);

            // Reset image.
            Image = oldImage;

            // Attempt to end the spritebatch
            _ = GameBase.Game.TryEndBatch();
        }
    }
}
