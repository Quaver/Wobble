//-----------------------------------------------------------------------------
// Copyright (c) 2008-2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Shaders
{
    /// <summary>
    /// A Gaussian blur filter kernel class. A Gaussian blur filter kernel is
    /// perfectly symmetrical and linearly separable. This means we can split
    /// the full 2D filter kernel matrix into two smaller horizontal and
    /// vertical 1D filter kernel matrices and then perform the Gaussian blur
    /// in two passes. Contrary to what you might think performing the Gaussian
    /// blur in this way is actually faster than performing the Gaussian blur
    /// in a single pass using the full 2D filter kernel matrix.
    /// <para>
    /// The GaussianBlur class is intended to be used in conjunction with an
    /// HLSL Gaussian blur shader. The following code snippet shows a typical
    /// Effect file implementation of a Gaussian blur.
    /// <code>
    /// #define RADIUS  7
    /// #define KERNEL_SIZE (RADIUS * 2 + 1)
    ///
    /// float weights[KERNEL_SIZE];
    /// float2 offsets[KERNEL_SIZE];
    ///
    /// texture colorMapTexture;
    ///
    /// sampler2D colorMap = sampler_state
    /// {
    ///     Texture = <![CDATA[<colorMapTexture>;]]>
    ///     MipFilter = Linear;
    ///     MinFilter = Linear;
    ///     MagFilter = Linear;
    /// };
    ///
    /// float4 PS_GaussianBlur(float2 texCoord : TEXCOORD) : COLOR0
    /// {
    ///     float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    ///
    ///     <![CDATA[for (int i = 0; i < KERNEL_SIZE; ++i)]]>
    ///         color += tex2D(colorMap, texCoord + offsets[i]) * weights[i];
    ///
    ///     return color;
    /// }
    ///
    /// technique GaussianBlur
    /// {
    ///     pass
    ///     {
    ///         PixelShader = compile ps_2_0 PS_GaussianBlur();
    ///     }
    /// }
    /// </code>
    /// The RADIUS constant in the effect file must match the radius value in
    /// the GaussianBlur class. The effect file's weights global variable
    /// corresponds to the GaussianBlur class' kernel field. The effect file's
    /// offsets global variable corresponds to the GaussianBlur class'
    /// offsetsHoriz and offsetsVert fields.
    /// </para>
    /// </summary>
    public class GaussianBlur
    {
        private Game game => GameBase.Game;
        private Effect effect;
        private int radius;
        private float amount;
        private float sigma;
        private float[] kernel;
        private Vector2[] offsetsHoriz;
        private Vector2[] offsetsVert;

        /// <summary>
        /// Returns the radius of the Gaussian blur filter kernel in pixels.
        /// </summary>
        public int Radius => radius;

        /// <summary>
        /// Returns the blur amount. This value is used to calculate the
        /// Gaussian blur filter kernel's sigma value. Good values for this
        /// property are 2 and 3. 2 will give a more blurred result whilst 3
        /// will give a less blurred result with sharper details.
        /// </summary>
        public float Amount => amount;

        /// <summary>
        /// Returns the Gaussian blur filter's standard deviation.
        /// </summary>
        public float Sigma => sigma;

        /// <summary>
        /// Returns the Gaussian blur filter kernel matrix. Note that the
        /// kernel returned is for a 1D Gaussian blur filter kernel matrix
        /// intended to be used in a two pass Gaussian blur operation.
        /// </summary>
        public float[] Kernel => kernel;

        /// <summary>
        /// Returns the texture offsets used for the horizontal Gaussian blur
        /// pass.
        /// </summary>
        public Vector2[] TextureOffsetsX => offsetsHoriz;

        /// <summary>
        /// Returns the texture offsets used for the vertical Gaussian blur
        /// pass.
        /// </summary>
        public Vector2[] TextureOffsetsY => offsetsVert;

        /// <summary>
        /// This overloaded constructor instructs the GaussianBlur class to
        /// load and use its GaussianBlur.fx effect file that implements the
        /// two pass Gaussian blur operation on the GPU. The effect file must
        /// be already bound to the asset name: 'Effects\GaussianBlur' or
        /// 'GaussianBlur'.
        /// </summary>
        public GaussianBlur(float strength)
        {
            effect = new Effect(GameBase.Game.GraphicsDevice, GameBase.Game.Resources.Get("Wobble.Resources/Shaders/fast-gaussian-blur.mgfxo"));
            ComputeKernel(7, strength);
        }

        /// <summary>
        /// Calculates the Gaussian blur filter kernel. This implementation is
        /// ported from the original Java code appearing in chapter 16 of
        /// "Filthy Rich Clients: Developing Animated and Graphical Effects for
        /// Desktop Java".
        /// </summary>
        /// <param name="blurRadius">The blur radius in pixels.</param>
        /// <param name="blurAmount">Used to calculate sigma.</param>
        public void ComputeKernel(int blurRadius, float blurAmount)
        {
            radius = blurRadius;
            amount = blurAmount;

            kernel = null;
            kernel = new float[radius * 2 + 1];
            sigma = radius / amount;

            var twoSigmaSquare = 2.0f * sigma * sigma;
            var sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            var total = 0.0f;
            var distance = 0.0f;
            var index = 0;

            for (var i = -radius; i <= radius; ++i)
            {
                distance = i * i;
                index = i + radius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (var i = 0; i < kernel.Length; ++i)
                kernel[i] /= total;
        }

        /// <summary>
        /// Calculates the texture coordinate offsets corresponding to the
        /// calculated Gaussian blur filter kernel. Each of these offset values
        /// are added to the current pixel's texture coordinates in order to
        /// obtain the neighboring texture coordinates that are affected by the
        /// Gaussian blur filter kernel. This implementation has been adapted
        /// from chapter 17 of "Filthy Rich Clients: Developing Animated and
        /// Graphical Effects for Desktop Java".
        /// </summary>
        /// <param name="textureWidth">The texture width in pixels.</param>
        /// <param name="textureHeight">The texture height in pixels.</param>
        public void ComputeOffsets(float textureWidth, float textureHeight)
        {
            offsetsHoriz = null;
            offsetsHoriz = new Vector2[radius * 2 + 1];

            offsetsVert = null;
            offsetsVert = new Vector2[radius * 2 + 1];

            var index = 0;
            var xOffset = 1.0f / textureWidth;
            var yOffset = 1.0f / textureHeight;

            for (var i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsetsHoriz[index] = new Vector2(i * xOffset, 0.0f);
                offsetsVert[index] = new Vector2(0.0f, i * yOffset);
            }
        }

        /// <summary>
        /// Performs the Gaussian blur operation on the source texture image.
        /// The Gaussian blur is performed in two passes: a horizontal blur
        /// pass followed by a vertical blur pass. The output from the first
        /// pass is rendered to renderTarget1. The output from the second pass
        /// is rendered to renderTarget2. The dimensions of the blurred texture
        /// is therefore equal to the dimensions of renderTarget2.
        /// </summary>
        /// <param name="srcTexture">The source image to blur.</param>
        /// <param name="renderTarget1">Stores the output from the horizontal blur pass.</param>
        /// <param name="renderTarget2">Stores the output from the vertical blur pass.</param>
        /// <param name="spriteBatch">Used to draw quads for the blur passes.</param>
        /// <returns>The resulting Gaussian blurred image.</returns>
        public Texture2D PerformGaussianBlur(Texture2D srcTexture)
        {
            if (effect == null)
                throw new InvalidOperationException("GaussianBlur.fx effect not loaded.");

            var renderTargetWidth = srcTexture.Width / 2;
            var renderTargetHeight = srcTexture.Height / 2;

            var renderTarget1 = new RenderTarget2D(GameBase.Game.GraphicsDevice, renderTargetWidth, renderTargetHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            var renderTarget2 = new RenderTarget2D(GameBase.Game.GraphicsDevice,renderTargetWidth, renderTargetHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            ComputeOffsets(renderTargetWidth, renderTargetHeight);

            Texture2D outputTexture = null;
            var srcRect = new Rectangle(0, 0, srcTexture.Width, srcTexture.Height);
            var destRect1 = new Rectangle(0, 0, renderTarget1.Width, renderTarget1.Height);
            var destRect2 = new Rectangle(0, 0, renderTarget2.Width, renderTarget2.Height);

            // Perform horizontal Gaussian blur.

            game.GraphicsDevice.SetRenderTarget(renderTarget1);

            effect.CurrentTechnique = effect.Techniques["GaussianBlur"];
            effect.Parameters["weights"].SetValue(kernel);
            effect.Parameters["colorMapTexture"].SetValue(srcTexture);
            effect.Parameters["offsets"].SetValue(offsetsHoriz);

            GameBase.Game.TryEndBatch();
            GameBase.Game.SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            GameBase.Game.SpriteBatch.Draw(srcTexture, destRect1, Color.White);
            GameBase.Game.SpriteBatch.End();

            // Perform vertical Gaussian blur.

            game.GraphicsDevice.SetRenderTarget(renderTarget2);
            outputTexture = renderTarget1;

            effect.Parameters["colorMapTexture"].SetValue(outputTexture);
            effect.Parameters["offsets"].SetValue(offsetsVert);

            GameBase.Game.SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            GameBase.Game.SpriteBatch.Draw(outputTexture, destRect2, Color.White);
            GameBase.Game.SpriteBatch.End();

            // Return the Gaussian blurred texture.

            game.GraphicsDevice.SetRenderTarget(null);
            outputTexture = renderTarget2;

            renderTarget1.Dispose();
            renderTarget1 = null;

            return outputTexture;
        }
    }
}