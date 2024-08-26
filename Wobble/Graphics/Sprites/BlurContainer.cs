using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics.Shaders;

namespace Wobble.Graphics.Sprites
{
    /// <inheritdoc />
    /// <summary>
    ///     Container for blurring sprites. Any child element will be placed under the same blur effect.
    /// </summary>
    public class BlurContainer : Container
    {
        /// <summary>
        ///     The type of blur to give it.
        /// </summary>
        public BlurType BlurType { get; set; }

        /// <summary>
        ///     The types of blur effects.
        /// </summary>
        private Dictionary<BlurType, Effect> BlurEffects { get; }

        /// <summary>
        ///     The strength of the blur.
        /// </summary>
        public float Strength { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///      Creates a new blur container
        /// </summary>
        public BlurContainer(BlurType blurType, float strength)
        {
            CastToRenderTarget();
            // ReSharper disable once ArrangeConstructorOrDestructorBody
            DefaultProjectionSprite.SpriteBatchOptions = new SpriteBatchOptions()
            {
                SortMode = SpriteSortMode.Deferred,
                BlendState = BlendState.NonPremultiplied,
                SamplerState = SamplerState.PointClamp,
                DepthStencilState = DepthStencilState.Default,
                RasterizerState = RasterizerState.CullNone,
            };

            // Load all three shaders.
            BlurEffects = new Dictionary<BlurType, Effect>
            {
                {BlurType.Gaussian, new Effect(GameBase.Game.GraphicsDevice, GameBase.Game.Resources.Get("Wobble.Resources/Shaders/gaussian-blur.mgfxo"))},
                {BlurType.Frosty, new Effect(GameBase.Game.GraphicsDevice, GameBase.Game.Resources.Get("Wobble.Resources/Shaders/frosty-blur.mgfxo"))},
                {BlurType.Fast, new Effect(GameBase.Game.GraphicsDevice, GameBase.Game.Resources.Get("Wobble.Resources/Shaders/fast-blur.mgfxo"))},
            };

            BlurType = blurType;
            Strength = strength;

            DefaultProjectionSprite.SpriteBatchOptions.Shader = new Shader(BlurEffects[BlurType], new Dictionary<string, object>
            {
                {"p_blurValues", new Vector3(Width, Height, Strength)}
            });
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Set to the correct blur type.
            DefaultProjectionSprite.SpriteBatchOptions.Shader.ShaderEffect = BlurEffects[BlurType];

            // Set dictionary parameters with updated properties.
            DefaultProjectionSprite.SpriteBatchOptions.Shader.SetParameter("p_blurValues", new Vector3(Width, Height, Strength), true);

            base.Draw(gameTime);
        }
    }

    /// <summary>
    ///     Enum containing the different types of blur.
    /// </summary>
    public enum BlurType
    {
        Gaussian,
        Frosty,
        Fast
    }
}