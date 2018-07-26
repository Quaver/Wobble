using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Wobble.Graphics.UI
{
    public class BackgroundImage : Sprite
    {
        /// <summary>
        ///     This sprite is overlayed on top of the actual background image
        ///     to give it a brightness effect.
        ///
        ///     TODO: Use a shader for this instead of drawing a new child sprite.
        /// </summary>
        private Sprite BrightnessSprite { get; }

        /// <summary>
        ///     The dim of the background as a percentage.
        /// </summary>
        private int _dim;
        public int Dim
        {
            get => _dim;
            set
            {
                value = MathHelper.Clamp(value, 0, 100);
                _dim = value;

                BrightnessSprite.Alpha = Dim / 100f;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image">The background image to use.</param>
        /// <param name="dim">The background dim as a percentage from 1-100%</param>
        public BackgroundImage(Texture2D image, int dim = 0)
        {
            Image = image;
            Size = new ScalableVector2(WindowManager.VirtualScreen.X + 100, WindowManager.VirtualScreen.Y + 100);

            BrightnessSprite = new Sprite
            {
                Image = WobbleAssets.WhiteBox,
                Tint = Color.Black,
                Parent = this,
                Size = Size,
                Alpha = dim / 100f
            };

            Dim = dim;

            // Hook onto the event when the resolution changes, so the background's size can change
            // accordingly.
            WindowManager.ResolutionChanged += OnResolutionChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            WindowManager.ResolutionChanged -= OnResolutionChanged;

            base.Destroy();
        }

        /// <summary>
        ///     When the resolution of the game changes, we'll want to change the size of the background
        ///     as well to fit the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResolutionChanged(object sender, EventArgs e)
        {
            Size = new ScalableVector2(WindowManager.VirtualScreen.X + 100, WindowManager.VirtualScreen.Y + 100);
            BrightnessSprite.Size = Size;
        }
    }
}