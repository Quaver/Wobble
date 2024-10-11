using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public SpriteSortMode SortMode { get; set; } = SpriteSortMode.Deferred;
        public BlendState BlendState { get; set; } = BlendState.NonPremultiplied;
        public SamplerState SamplerState { get; set; } = SamplerState.LinearClamp;
        public DepthStencilState DepthStencilState { get; set; }
        public RasterizerState RasterizerState { get; set; }
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
            GameBase.Game.SpriteBatch.Begin(SortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Shader?.ShaderEffect, matrix);
        }
    }
}
