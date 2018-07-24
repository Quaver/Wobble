using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites
{
    public class BakeableSprite : Sprite
    {
        /// <summary>
        ///     This is the render target in which all sprites will be baked to.
        /// </summary>
        private RenderTarget2D BakedRenderTarget { get; set; }

        /// <summary>
        ///     Dictates if the object has been baked yet.
        /// </summary>
        public bool HasBeenBaked { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // First update the object.
            base.Update(gameTime);

            // Bake the object after it has been updated once.
            if (!HasBeenBaked)
                Bake(gameTime);
        }

        /// <summary>
        ///     Bakes all the sprite and all of its children onto an object
        ///     then gets rid of the children.
        /// </summary>
        private void Bake(GameTime gameTime)

        {
            var game = GameBase.Game;

            BakedRenderTarget = new RenderTarget2D(game.GraphicsDevice, (int)Math.Ceiling(AbsoluteSize.X), (int)Math.Ceiling(AbsoluteSize.Y));
            Alpha = 0;

            // Set the new render target
            game.GraphicsDevice.SetRenderTarget(BakedRenderTarget);
            game.GraphicsDevice.Clear(Color.White * 0);

            // Draw the sprite to the render target.
            game.SpriteBatch.Begin();
            Draw(gameTime);
            game.SpriteBatch.End();

            // Reset the render target back to the backbuffer.
            game.GraphicsDevice.SetRenderTarget(null);

            // Destroy all children (which removes them from the list as well)
            for (var i = Children.Count - 1; i >= 0; i--)
                Children[i].Destroy();

            // Set the image to the baked render target.
            Image = BakedRenderTarget;
            Alpha = 1;

            HasBeenBaked = true;
        }
    }
}
