using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    /// <summary>
    ///     Sprite used to draw things to a different RenderTarget.
    /// </summary>
    public class RenderTargetContainer : Container
    {
        /// <summary>
        ///     The main render target to render to.
        /// </summary>
        public RenderTarget2D RenderTarget { get; set; }

        protected Sprite RenderSprite { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public RenderTargetContainer(ScalableVector2 size)
        {
            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int)size.X.Value, (int)size.Y.Value);
            RenderSprite = new Sprite
            {
                Size = size,
                Position = Position,
                Rotation = Rotation,
                Image = RenderTarget,
                Alignment = Alignment
            };
            Size = size;
        }

        /// <summary>
        /// </summary>
        public RenderTargetContainer() : this(new ScalableVector2(WindowManager.Width, WindowManager.Height))
        {
        }

        protected override void RecalculateTransformMatrix()
        {
            ChildPositionTransform = Matrix2D.Identity;
            ChildRelativeTransform = Matrix2D.Identity;
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (RenderSprite == null)
                return;
            RenderSprite.Parent = Parent;
            RenderSprite.Size = Size;
            RenderSprite.Scale = Scale;
            RenderSprite.Rotation = Rotation;
            RenderSprite.Alignment = Alignment;
            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice,
                (int)RelativeRectangle.Width, (int)RelativeRectangle.Height);
            RenderSprite.Image = RenderTarget;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.TryEndBatch();
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

            // Attempt to end the spritebatch
            _ = GameBase.Game.TryEndBatch();
        }
    }
}