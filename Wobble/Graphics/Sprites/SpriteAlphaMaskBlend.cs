using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Wobble.Graphics.Sprites
{
    public class SpriteAlphaMaskBlend
    {
        static private Game game => GameBase.Game;

        static private readonly BlendState blend = new BlendState
        {
            AlphaSourceBlend = Blend.DestinationAlpha,
            AlphaBlendFunction = BlendFunction.Subtract,
            AlphaDestinationBlend = Blend.InverseDestinationAlpha
        };

        static public Texture2D PerformBlend(Texture2D srcTexture, Texture2D srcMask)
        {
            var renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, srcTexture.Width, srcTexture.Height, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            game.GraphicsDevice.SetRenderTarget(renderTarget);

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
            GameBase.Game.SpriteBatch.Draw(srcMask, srcTexture.Bounds, Color.White);
            GameBase.Game.SpriteBatch.Draw(srcTexture, srcTexture.Bounds, Color.White);
            GameBase.Game.SpriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);
            game.GraphicsDevice.Clear(Color.Black);

            return renderTarget;
        }
    }
}
