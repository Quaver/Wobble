using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Graphics.Shaders;
using Wobble.Window;

namespace Wobble.Graphics
{
    /// <summary>
    ///     Class that defines the options to use on SpriteBatch.Begin();
    ///     If
    /// </summary>
    public class SpriteBatchOptions
    {
        private static readonly RasterizerState DefaultScissorRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        private static readonly Dictionary<RasterizerState, RasterizerState> ScissorRasterizerStates =
            new Dictionary<RasterizerState, RasterizerState>();

        /// <summary>
        ///     Temporary virtual-screen clip inherited by nested draw scopes. Render targets are intentionally excluded
        ///     because their coordinates are local to the target.
        /// </summary>
        internal static Rectangle? ActiveClipRectangle { get; set; }

        internal static Rectangle? PushClip(RectangleF rectangle)
        {
            var previousClip = ActiveClipRectangle;
            var left = (int) Math.Floor(rectangle.Left);
            var top = (int) Math.Floor(rectangle.Top);
            var right = (int) Math.Ceiling(rectangle.Right);
            var bottom = (int) Math.Ceiling(rectangle.Bottom);
            var clip = new Rectangle(left, top, right - left, bottom - top);

            if (previousClip.HasValue)
                clip = Rectangle.Intersect(previousClip.Value, clip);

            ActiveClipRectangle = clip;
            return previousClip;
        }

        internal static void RestoreClip(Rectangle? rectangle) => ActiveClipRectangle = rectangle;

        public SpriteSortMode SortMode { get; set; } = SpriteSortMode.Deferred;
        public BlendState BlendState { get; set; } = BlendState.NonPremultiplied;
        public SamplerState SamplerState { get; set; } = SamplerState.LinearClamp;
        public DepthStencilState DepthStencilState { get; set; }
        public RasterizerState RasterizerState { get; set; } = RasterizerState.CullNone;
        /// <summary>
        ///     Custom shader for this sprite.
        /// </summary>
        private Shader _shader;
        public Shader Shader
        {
            get => _shader;
            set
            {
                // Dispose the shader if we already have one loaded.
                if (Shader != null && !Shader.IsDisposed)
                    Shader.Dispose();

                _shader = value;
            }
        }

        public bool DoNotScale = false;

        /// <summary>
        ///     Begins the spritebatch with the specified settings.
        /// </summary>
        public void Begin()
        {
            Matrix? matrix = WindowManager.Scale;

            if (DoNotScale)
                matrix = null;

            _ = GameBase.Game.TryEndBatch();

            var rasterizerState = RasterizerState;
            var graphicsDevice = GameBase.Game.GraphicsDevice;

            if (ActiveClipRectangle.HasValue && graphicsDevice.GetRenderTargets().Length == 0)
            {
                var clipRectangle = ToBackBufferRectangle(ActiveClipRectangle.Value);

                if (graphicsDevice.RasterizerState?.ScissorTestEnable == true)
                    clipRectangle = Rectangle.Intersect(clipRectangle, graphicsDevice.ScissorRectangle);

                graphicsDevice.ScissorRectangle = Rectangle.Intersect(clipRectangle, graphicsDevice.Viewport.Bounds);
                rasterizerState = GetScissorRasterizerState(rasterizerState);
            }

            GameBase.Game.BeginBatch(SortMode, BlendState, SamplerState, DepthStencilState, rasterizerState,
                Shader?.ShaderEffect, matrix);
        }

        private static Rectangle ToBackBufferRectangle(Rectangle rectangle)
        {
            var left = (int) Math.Floor(rectangle.Left * WindowManager.ScreenScale.X);
            var top = (int) Math.Floor(rectangle.Top * WindowManager.ScreenScale.Y);
            var right = (int) Math.Ceiling(rectangle.Right * WindowManager.ScreenScale.X);
            var bottom = (int) Math.Ceiling(rectangle.Bottom * WindowManager.ScreenScale.Y);
            return new Rectangle(left, top, right - left, bottom - top);
        }

        private static RasterizerState GetScissorRasterizerState(RasterizerState rasterizerState)
        {
            if (rasterizerState == null)
                return DefaultScissorRasterizerState;

            if (rasterizerState.ScissorTestEnable)
                return rasterizerState;

            if (ScissorRasterizerStates.TryGetValue(rasterizerState, out var scissorRasterizerState))
                return scissorRasterizerState;

            scissorRasterizerState = new RasterizerState
            {
                CullMode = rasterizerState.CullMode,
                FillMode = rasterizerState.FillMode,
                DepthBias = rasterizerState.DepthBias,
                MultiSampleAntiAlias = rasterizerState.MultiSampleAntiAlias,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = rasterizerState.SlopeScaleDepthBias
            };
            ScissorRasterizerStates.Add(rasterizerState, scissorRasterizerState);
            return scissorRasterizerState;
        }
    }
}
