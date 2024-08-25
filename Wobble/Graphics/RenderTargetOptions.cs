using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Bindables;
using Wobble.Window;

namespace Wobble.Graphics
{
    /// <summary>
    ///     Specification of a render target
    /// </summary>
    public class RenderTargetOptions
    {
        private Rectangle _renderRectangle;
        private bool _enabled;
        private Point _containerRectangleSize;
        private Padding _overflowRenderPadding;
        private Matrix2 _transformMatrix;

        /// <summary>
        ///     Whether the render target should be and is being used.
        ///     When enabling, <see cref="RenderTarget"/> will be refreshed if the dimensions don't match or if it's null.
        ///     When disabling, <see cref="RenderTarget"/> will be disposed
        ///     Both actions will trigger <see cref="Bindable{T}.ValueChanged"/> of <see cref="RenderTarget"/>
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (value)
                {
                    ResetRenderTarget();
                }
                else
                {
                    RenderTarget.Value?.Dispose();
                    RenderTarget.Value = null;
                }
            }
        }

        public Bindable<RenderTarget2D> RenderTarget { get; } = new Bindable<RenderTarget2D>(null);

        /// <summary>
        ///     The size of the container itself. This does not include the overflow area.
        /// </summary>
        public Point ContainerRectangleSize
        {
            get => _containerRectangleSize;
            set
            {
                if (_containerRectangleSize == value)
                    return;
                _containerRectangleSize = value;
                _renderRectangle = _overflowRenderPadding.PadOutwards(new Rectangle(Point.Zero, value));
                RecalculateTransformMatrix();
            }
        }

        /// <summary>
        ///     Padding *outwards* from <see cref="ContainerRectangleSize"/>
        ///     It and <see cref="ContainerRectangleSize"/> combined gives <see cref="RenderRectangle"/>.
        ///     For example, in quaver the playfield container's rectangle doesn't contain every child in its rectangle.
        ///     i.e. the actual rendering needs to be done on a larger render rectangle.
        ///     Adding padding makes the extra rendering visible.
        /// </summary>
        public Padding OverflowRenderPadding
        {
            get => _overflowRenderPadding;
            set
            {
                if (_overflowRenderPadding == value)
                    return;
                _overflowRenderPadding = value;
                _renderRectangle = value.PadOutwards(new Rectangle(Point.Zero, _containerRectangleSize));
                RecalculateTransformMatrix();
                ResetRenderTarget();
            }
        }

        /// <summary>
        ///     Absolute translation needed for the children of the container to be drawn.
        /// </summary>
        public Vector2 RenderOffset { get; private set; }

        /// <summary>
        ///     Relative rectangle of the full render target
        /// </summary>
        public Rectangle RenderRectangle => _renderRectangle;

        /// <summary>
        ///     Matrix needed to translate the position of children.
        ///     This includes translation by <see cref="RenderOffset"/>
        ///     followed by inverse scaling of <see cref="WindowManager.ScreenScale"/>.
        /// </summary>
        public Matrix2 TransformMatrix => _transformMatrix;

        /// <summary>
        ///     When rendering to <see cref="RenderTarget"/>, the background color to give.
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Transparent;

        // SpriteBatchOptions will scale thing to WindowManager.ScreenScale, but out render target is already
        // scaled, so we should scale them back.
        public Vector2 Scale { get; private set; }

        public void RecalculateTransformMatrix()
        {
            Scale = new Vector2(1 / WindowManager.ScreenScale.X, 1 / WindowManager.ScreenScale.Y);
            RenderOffset = new Vector2(-_renderRectangle.X, -_renderRectangle.Y);
            var offsetTranslation = Matrix2.CreateTranslation(RenderOffset);
            var scalingMatrix = Matrix2.CreateScale(Scale);
            Matrix2.Multiply(ref offsetTranslation, ref scalingMatrix, out _transformMatrix);
        }

        /// <summary>
        ///     Refreshes the render target.
        ///     Since creating a <see cref="RenderTarget2D"/> is expensive, it's only done when <see cref="RenderTarget"/>
        ///     has a null value or when the dimensions don't match.
        /// </summary>
        public void ResetRenderTarget()
        {
            if (!Enabled)
                return;

            if (RenderTarget.Value != null && RenderTarget.Value.Bounds.Size == _renderRectangle.Size)
                return;

            RenderTarget.Value = new RenderTarget2D(GameBase.Game.GraphicsDevice,
                _renderRectangle.Width, _renderRectangle.Height, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
        }
    }
}