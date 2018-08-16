using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites
{
    /// <summary>
    ///     Sprite used to draw things to a different RenderTarget.
    /// </summary>
    public class RenderTargetContainer : Sprite
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Create a new render target and set the game's current RT to it.
            var rt = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) Width, (int) Height);
            GameBase.Game.GraphicsDevice.SetRenderTarget(rt);
            GameBase.Game.GraphicsDevice.Clear(Color.Black);

            // Draw all of the children
            Children.ForEach(x => x.Draw(gameTime));

            // Attempt to end the spritebatch
            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception e)
            {
                // ignored.
            }

            // Reset the render target.
            GameBase.Game.GraphicsDevice.SetRenderTarget(null);

            // Grab the old image so we can set it back later.
            var oldImage = Image;

            // Change the image to the render target
            // and draw it.
            Image = rt;
            base.Draw(gameTime);

            // Reset image.
            Image = oldImage;
        }
    }
}