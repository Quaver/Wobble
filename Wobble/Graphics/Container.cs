using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Wobble.Graphics
{
    /// <inheritdoc />
    /// <summary>
    ///     Container as a parent for sprites to easily lay them out.
    ///     Default size is the virtual screen resolution.
    /// </summary>
    public class Container : Drawable
    {
        /// <summary>
        ///     The main render target to render to.
        /// </summary>
        public RenderTargetOptions RenderTargetOptions { get; } = new RenderTargetOptions();

        protected override RectangleF ChildDrawRectangleMask =>
            RenderTargetOptions.RenderTarget.Value?.Bounds 
            ?? base.ChildDrawRectangleMask;

        /// <summary>
        ///     A projection sprite that has the same dimension, position, rotation and parent as the container.
        ///     It shows <see cref="RenderTarget"/>, which the container can render its entire content to
        /// </summary>
        public RenderProjectionSprite DefaultProjectionSprite { get; private set; }

        public Container()
        {
            Size = new ScalableVector2(WindowManager.Rectangle.Width, WindowManager.Rectangle.Height);
            Position = new ScalableVector2(0, 0);
        }

        public Container(ScalableVector2 size, ScalableVector2 position)
        {
            Size = size;
            Position = position;
        }

        public Container(float x, float y, float width, float height)
        {
            Size = new ScalableVector2(width, height);
            Position = new ScalableVector2(x, y);
        }

        protected override void RecalculateTransformMatrix()
        {
            if (RenderTargetOptions.RenderTarget.Value == null)
            {
                base.RecalculateTransformMatrix();
            }
            else
            {
                // SpriteBatchOptions will scale thing to WindowManager.ScreenScale, but out render target is already
                // scaled, so we should scale them back.
                ChildRelativeTransform = RenderTargetOptions.TransformMatrix;
                ChildPositionTransform = ChildRelativeTransform;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (Parent == null)
                DefaultProjectionSprite?.Draw(gameTime);
            if (RenderTargetOptions.RenderTarget.Value != null)
                GameBase.Game.ScheduledRenderTargetDraws.Add(DrawToRenderTarget);
            else
                base.Draw(gameTime);
        }

        /// <summary>
        ///     Draw this container to a render target so its view can be duplicated and shown in
        ///     a different way.
        ///     **THIS CAN CAUSE PERFORMANCE DEGREDATION**
        /// </summary>
        /// <remarks>
        ///     The render target is bounded by the size of the container, so
        ///     anything outside this container will be clipped
        /// </remarks>
        /// <param name="projectDefault">Whether a sprite will be spawned to show the container as normal</param>
        public void CastToRenderTarget(bool projectDefault = true)
        {
            RenderTargetOptions.ContainerRectangleSize =
                new Point((int)RelativeRectangle.Size.Width, (int)RelativeRectangle.Size.Height);
            RenderTargetOptions.Enabled = true;
            RecalculateRectangles();

            DefaultProjectionSprite?.Destroy();

            if (projectDefault)
            {
                DefaultProjectionSprite = new RenderProjectionSprite
                {
                    Size = Size,
                    Position = Position,
                    Rotation = Rotation,
                    Alignment = Alignment,
                    Parent = Parent
                };
                DefaultProjectionSprite.BindProjectionContainer(this);
            }
        }

        public void StopCasting()
        {
            DefaultProjectionSprite?.Destroy();
            RenderTargetOptions.Enabled = false;
            DefaultProjectionSprite = null;
            RecalculateRectangles();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (DefaultProjectionSprite != null)
            {
                DefaultProjectionSprite.Parent = Parent;
                DefaultProjectionSprite.Size = Size;
                DefaultProjectionSprite.Scale = Scale;
                DefaultProjectionSprite.Rotation = Rotation;
                DefaultProjectionSprite.Position = Position;
                DefaultProjectionSprite.Alignment = Alignment;
            }

            RenderTargetOptions.ContainerRectangleSize = new Point((int)RelativeRectangle.Width, (int)RelativeRectangle.Height);
            RenderTargetOptions.ResetRenderTarget();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Simply a container. There's no need to draw it to spritebatch.
        /// </summary>
        public override void DrawToSpriteBatch()
        {
        }

        private void DrawToRenderTarget(GameTime gameTime)
        {
            if (RenderTargetOptions.RenderTarget.Value == null)
                return;

            GameBase.Game.TryEndBatch();
            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTargetOptions.RenderTarget.Value);
            GameBase.Game.GraphicsDevice.Clear(RenderTargetOptions.BackgroundColor);

            // Draw all of the children
            Children.ForEach(x =>
            {
                x.UsePreviousSpriteBatchOptions = true;
                x.Draw(gameTime);
            });

            // Attempt to end the spritebatch
            _ = GameBase.Game.TryEndBatch();
        }
    }
}