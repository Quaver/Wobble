using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    /// <summary>
    ///     Used as a container for masking sprites in images.
    ///     Documentation: https://gamedev.stackexchange.com/questions/38118/best-way-to-mask-2d-sprites-in-xna
    /// </summary>
    public class SpriteMaskContainer : Sprite
    {
        /// <summary>
        ///     The depth stencil state of the mask itself
        /// </summary>
        public DepthStencilState MaskDepthStencilState { get; }

        /// <summary>
        ///     Depth stencil state of the contained sprites.
        /// </summary>
        public DepthStencilState ContainedDepthStencilState { get; }

        /// <summary>
        ///     Matrix for AlphaTestEffect
        /// </summary>
        private Matrix Matrix { get; }

        /// <summary>
        ///
        /// </summary>
        public AlphaTestEffect MaskAlphaTestEffect { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SpriteMaskContainer()
        {
            MaskDepthStencilState = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            ContainedDepthStencilState = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            Matrix = Matrix.CreateOrthographicOffCenter(0, WindowManager.Width, WindowManager.Height, 0, 0, 1);

            MaskAlphaTestEffect = new AlphaTestEffect(GameBase.Game.Graphics.GraphicsDevice)
            {
                Projection = Matrix,
            };

            SpriteBatchOptions = new SpriteBatchOptions
            {
                DepthStencilState = MaskDepthStencilState,
                Shader = new Shader(MaskAlphaTestEffect, new Dictionary<string, object>())
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            MaskAlphaTestEffect.Alpha = Alpha;
            base.Update(gameTime);
        }

        /// <summary>
        ///     Adds a contained drawable to the mask container.
        /// </summary>
        /// <param name="drawable"></param>
        public void AddContainedSprite(Drawable drawable)
        {
            // Add the sprite as a child.
            drawable.Parent = this;

            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                // The first child is always the one to have the new sprite batch options.
                if (i == 0)
                {
                    child.SpriteBatchOptions = new SpriteBatchOptions
                    {
                        DepthStencilState = ContainedDepthStencilState,
                        Shader = new Shader(CreateAlphaTestEffect(drawable), new Dictionary<string, object>())
                    };
                }
                // All other children need to use previous SpriteBatch options.
                else
                {
                    child.UsePreviousSpriteBatchOptions = true;
                }
            }
        }

        /// <summary>
        ///     Creates an AlphaTestEffect from a drawable.
        /// </summary>
        /// <param name="drawable"></param>
        /// <returns></returns>
        private AlphaTestEffect CreateAlphaTestEffect(Drawable drawable)
        {
            var alpha = 0f;

            if (drawable is Sprite sprite)
            {
                alpha = sprite.Alpha;
            }
            return new AlphaTestEffect(GameBase.Game.GraphicsDevice)
            {
                Projection = Matrix,
                Alpha = alpha,
            };
        }
    }
}