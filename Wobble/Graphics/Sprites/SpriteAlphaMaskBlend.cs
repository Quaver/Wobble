using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Wobble.Graphics.Sprites
{
    public class SpriteAlphaMaskBlend : Sprite
    {
        private RenderTarget2D RenderTarget { get; set; }

        private readonly BlendState blend = new BlendState
        {
            AlphaSourceBlend = Blend.DestinationAlpha,
            AlphaBlendFunction = BlendFunction.Subtract,
            AlphaDestinationBlend = Blend.InverseDestinationAlpha
        };

        public Texture2D PerformBlend(Texture2D srcTexcture, Texture2D srcMask)
        {
            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, srcTexcture.Width, srcTexcture.Height, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);

            // Attempt to end the spritebatch
            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception e)
            {
                // ignored.
            }

            GameBase.Game.SpriteBatch.Begin(blendState: blend);
            GameBase.Game.SpriteBatch.Draw(srcMask, srcTexcture.Bounds, Color.White);
            GameBase.Game.SpriteBatch.Draw(srcTexcture, srcTexcture.Bounds, Color.White);
            GameBase.Game.SpriteBatch.End();

            GameBase.Game.GraphicsDevice.SetRenderTarget(null);
            GameBase.Game.GraphicsDevice.Clear(Color.Black);

            return RenderTarget;
        }

        public override void Destroy()
        {
            if (RenderTarget != null)
            {
                RenderTarget.Dispose();
                RenderTarget = null;
            }
        }
    }
}
