using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Bindables;

namespace Wobble.Graphics.Sprites
{
    public class RenderProjectionSprite : Sprite
    {
        private Drawable _boundProjectionContainerSource;

        protected override void OnRectangleRecalculated()
        {
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible || _boundProjectionContainerSource == null)
                return;

            var matrix = Transform.SelfWorldMatrix.Matrix;
            var vertices = new Vector3[4];
            Array.Copy(_boundProjectionContainerSource.RenderTargetOptions.RelativeVertices, vertices, 4);
            var rectangleSize = _boundProjectionContainerSource.RenderTargetOptions.ContainerRectangleSize.ToVector2();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].X / rectangleSize.X * RelativeRectangle.Width,
                    vertices[i].Y / rectangleSize.Y * RelativeRectangle.Height, 0);
            }

            GameBase.Game.SpriteBatch.Draw(Image, vertices, ref matrix, null, AbsoluteColor,
                SpriteEffect);
        }

        public override void Destroy()
        {
            base.Destroy();
            if (_boundProjectionContainerSource != null)
            {
                _boundProjectionContainerSource.RenderTargetOptions.RenderTarget.ValueChanged -= OnRenderTargetChange;
                _boundProjectionContainerSource.SizeChanged -= ContainerSizeChanged;
            }
        }

        /// <summary>
        ///     When called, the sprite will show the image of the container instead.
        ///     If the container is not drawing to render target, it will automatically do so
        /// </summary>
        /// <param name="container">The container to project its drawing from</param>
        public void BindProjectionContainer(Drawable container)
        {
            if (_boundProjectionContainerSource != null)
                _boundProjectionContainerSource.RenderTargetOptions.RenderTarget.ValueChanged -= OnRenderTargetChange;

            _boundProjectionContainerSource = container;

            if (_boundProjectionContainerSource.RenderTargetOptions.RenderTarget?.Value == null)
                _boundProjectionContainerSource.CastToRenderTarget();

            SetRenderTarget(_boundProjectionContainerSource.RenderTargetOptions.RenderTarget?.Value);
            container.RenderTargetOptions.RenderTarget.ValueChanged += OnRenderTargetChange;
            container.SizeChanged += ContainerSizeChanged;
        }

        private void ContainerSizeChanged(object sender, ScalableVector2 e)
        {
            UpdateShaderSizeParameter();
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
            SpriteBatchOptions?.Shader?.TrySetParameter("UVToSize", size, true);
            size.X = 1 / size.X;
            size.Y = 1 / size.Y;
            SpriteBatchOptions?.Shader?.TrySetParameter("SizeToUV", size, true);
        }
    }
}