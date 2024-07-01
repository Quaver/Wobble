using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Bindables;

namespace Wobble.Graphics.Sprites
{
    public class RenderProjectionSprite : Sprite
    {
        private Container _boundProjectionContainerSource;

        protected override void OnRectangleRecalculated()
        {
            if (Image == null || _boundProjectionContainerSource == null)
                return;

            Origin = Pivot * _boundProjectionContainerSource.RenderTargetOptions.ContainerRectangleSize.ToVector2()
                     + _boundProjectionContainerSource.RenderTargetOptions.RenderOffset;

            // The render rectangle's position will rotate around the screen rectangle's position
            var rotatedScreenOrigin =
                (ScreenRectangle.Size * Pivot)
                .Rotate(Parent?.AbsoluteRotation ?? 0);

            // Update the render rectangle
            RenderRectangle = new RectangleF(
                ScreenRectangle.Position + rotatedScreenOrigin,
                ScreenRectangle.Size *
                _boundProjectionContainerSource.RenderTargetOptions.RenderRectangle.Size.ToVector2() /
                _boundProjectionContainerSource.RenderTargetOptions.ContainerRectangleSize.ToVector2());

            SpriteRotation = IndependentRotation ? Rotation : AbsoluteRotation;
        }

        public override void Destroy()
        {
            base.Destroy();
            if (_boundProjectionContainerSource != null)
                _boundProjectionContainerSource.RenderTargetOptions.RenderTarget.ValueChanged -= OnRenderTargetChange;
        }

        /// <summary>
        ///     When called, the sprite will show the image of the container instead.
        ///     If the container is not drawing to render target, it will automatically do so
        /// </summary>
        /// <param name="container">The container to project its drawing from</param>
        public void BindProjectionContainer(Container container)
        {
            if (_boundProjectionContainerSource != null)
                _boundProjectionContainerSource.RenderTargetOptions.RenderTarget.ValueChanged -= OnRenderTargetChange;

            _boundProjectionContainerSource = container;

            if (_boundProjectionContainerSource.RenderTargetOptions.RenderTarget?.Value == null)
                _boundProjectionContainerSource.CastToRenderTarget();

            SetRenderTarget(_boundProjectionContainerSource.RenderTargetOptions.RenderTarget?.Value);
            container.RenderTargetOptions.RenderTarget.ValueChanged += OnRenderTargetChange;
        }

        private void OnRenderTargetChange(object sender, BindableValueChangedEventArgs<RenderTarget2D> target2D)
        {
            SetRenderTarget(target2D.Value);
        }

        private void SetRenderTarget(RenderTarget2D renderTarget2D)
        {
            Image = renderTarget2D;
            UpdateShaderSizeParameter();
        }

        public void UpdateShaderSizeParameter()
        {
            var size = (Image?.Bounds.Size ?? new Point(1, 1)).ToVector2();
            SpriteBatchOptions?.Shader?.TrySetParameter("p_rendertarget_uvtosize", size, true);
            size.X = 1 / size.X;
            size.Y = 1 / size.Y;
            SpriteBatchOptions?.Shader?.TrySetParameter("p_rendertarget_sizetouv", size, true);
        }
    }
}