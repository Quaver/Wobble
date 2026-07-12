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
        private RenderTarget2D RenderTarget { get; set; }

        /// <summary>
        ///     Number of render-target allocations made by this container. Useful for diagnostics.
        /// </summary>
        public int RenderTargetGeneration { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public RenderTargetContainer(ScalableVector2 size)
        {
            Size = size;
            RecreateRenderTarget();
        }

        /// <summary>
        /// </summary>
        public RenderTargetContainer()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            RecreateRenderTarget();
        }

        private void EnsureRenderTarget()
        {
            var width = GetTargetWidth();
            var height = GetTargetHeight();

            if (RenderTarget == null || RenderTarget.IsDisposed || RenderTarget.IsContentLost ||
                RenderTarget.GraphicsDevice != GameBase.Game.GraphicsDevice ||
                RenderTarget.Width != width || RenderTarget.Height != height)
                RecreateRenderTarget();
        }

        /// <summary>
        ///     Disposes and recreates the underlying GPU render target without changing this
        ///     container's children, parent, or draw order.
        /// </summary>
        public void RecreateRenderTarget()
        {
            RenderTarget?.Dispose();
            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice,
                GetTargetWidth(), GetTargetHeight(), false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.PreserveContents);
            RenderTargetGeneration++;
        }

        private int GetTargetWidth() =>
            Math.Max(1, (int)Math.Ceiling(Width * WindowManager.ScreenScale.X));

        private int GetTargetHeight() =>
            Math.Max(1, (int)Math.Ceiling(Height * WindowManager.ScreenScale.Y));

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            EnsureRenderTarget();
            _ = GameBase.Game.TryEndBatch();
            GameBase.DefaultSpriteBatchInUse = false;

            var previousTargets = GameBase.Game.GraphicsDevice.GetRenderTargets();
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
            GameBase.DefaultSpriteBatchInUse = false;

            // Reset the render target.
            GameBase.Game.GraphicsDevice.SetRenderTargets(previousTargets);

            // Grab the old image so we can set it back later.
            var oldImage = Image;

            // Change the image to the render target
            // and draw it.
            Image = RenderTarget;
            var renderedChildren = new Drawable[Children.Count];
            Children.CopyTo(renderedChildren);
            Children.Clear();

            try
            {
                base.Draw(gameTime);
            }
            finally
            {
                Children.AddRange(renderedChildren);
            }

            // Reset image.
            Image = oldImage;

            // Attempt to end the spritebatch
            _ = GameBase.Game.TryEndBatch();
            GameBase.DefaultSpriteBatchInUse = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (RenderTarget != null && !RenderTarget.IsDisposed)
                RenderTarget.Dispose();

            base.Destroy();
        }
    }
}
