using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Window;

namespace Wobble.Graphics
{
    /// <summary>
    ///     A container that clips its children to its screen rectangle.
    /// </summary>
    public class HorizontalClippingContainer : Container
    {
        private static readonly RasterizerState ScissorRasterizer = new RasterizerState
        {
            ScissorTestEnable = true,
            CullMode = CullMode.None
        };

        public HorizontalClippingContainer() => UsePreviousSpriteBatchOptions = true;

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || ScreenRectangle.Width <= 0 || ScreenRectangle.Height <= 0)
                return;

            var game = GameBase.Game;
            var graphicsDevice = game?.GraphicsDevice;
            if (graphicsDevice == null)
                return;

            var viewport = graphicsDevice.Viewport;
            var scissorRectangle = new Rectangle(
                (int)(ScreenRectangle.X * WindowManager.ScreenScale.X),
                (int)(ScreenRectangle.Y * WindowManager.ScreenScale.Y),
                (int)(ScreenRectangle.Width * WindowManager.ScreenScale.X),
                (int)(ScreenRectangle.Height * WindowManager.ScreenScale.Y));

            scissorRectangle = Rectangle.Intersect(scissorRectangle, viewport.Bounds);
            if (scissorRectangle.Width <= 0 || scissorRectangle.Height <= 0)
                return;

            var oldScissorRectangle = graphicsDevice.ScissorRectangle;
            var oldRasterizerState = graphicsDevice.RasterizerState;
            var wasNestedScissor = oldRasterizerState?.ScissorTestEnable == true;

            if (wasNestedScissor)
                scissorRectangle = Rectangle.Intersect(scissorRectangle, oldScissorRectangle);

            if (scissorRectangle.Width <= 0 || scissorRectangle.Height <= 0)
                return;

            _ = game.TryEndBatch();

            try
            {
                graphicsDevice.ScissorRectangle = scissorRectangle;
                BeginScissorBatch();

                base.Draw(gameTime);
            }
            finally
            {
                _ = game.TryEndBatch();
                graphicsDevice.ScissorRectangle = oldScissorRectangle;

                if (wasNestedScissor)
                {
                    BeginScissorBatch();
                }
                else
                {
                    GameBase.DefaultSpriteBatchOptions.Begin();
                    GameBase.DefaultSpriteBatchInUse = true;
                }
            }
        }

        private static void BeginScissorBatch()
        {
            var options = GameBase.DefaultSpriteBatchOptions;
            var transform = options.DoNotScale ? (Matrix?)null : WindowManager.Scale;

            GameBase.Game.SpriteBatch.Begin(options.SortMode, options.BlendState, options.SamplerState,
                options.DepthStencilState, ScissorRasterizer, options.Shader?.ShaderEffect, transform);
            GameBase.DefaultSpriteBatchInUse = false;
        }
    }
}
