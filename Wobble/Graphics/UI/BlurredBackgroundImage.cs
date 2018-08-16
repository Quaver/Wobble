using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI
{
    public class BlurredBackgroundImage : BlurContainer
    {
        /// <summary>
        ///     The background sprite to be blurred.
        /// </summary>
        public BackgroundImage Sprite { get; }

        /// <summary>
        ///     Sets the background dim.
        /// </summary>
        public int Dim
        {
            get => Sprite.Dim;
            set => Sprite.Dim = value;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="blurType"></param>
        /// <param name="strength"></param>
        /// <param name="dim"></param>
        public BlurredBackgroundImage(Texture2D image, BlurType blurType = BlurType.Gaussian, float strength = 0, int dim = 0)
            : base(blurType, strength) => Sprite = new BackgroundImage(image, dim) { Parent = this };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // If somoene tries to set a transformation to this and not the child background sprite
            // then throw an exception.
            if (Transformations.Count > 0)
                throw new InvalidOperationException("Transformations cannot be applied to BlurredBackgroundImage. Only BlurredBackgroundImage.Background.");

            base.Update(gameTime);
        }
    }
}