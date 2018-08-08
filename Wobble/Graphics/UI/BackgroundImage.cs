using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics.Sprites;
using Wobble.Input;
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
        public Sprite BrightnessSprite { get; }

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

        /// <summary>
        ///     Determines if we this background image has a parallax effect
        ///     when moving the mouse cursor.
        /// </summary>
        public bool HasParallaxEffect { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image">The background image to use.</param>
        /// <param name="hasParallaxEffect">If the background will move when the mouse cursor does.</param>
        /// <param name="dim">The background dim as a percentage from 1-100%</param>
        public BackgroundImage(Texture2D image, int dim = 0, bool hasParallaxEffect = true)
        {
            Image = image;
            Size = new ScalableVector2(WindowManager.VirtualScreen.X + 100, WindowManager.VirtualScreen.Y + 100);

            BrightnessSprite = new Sprite
            {
                Image = WobbleAssets.WhiteBox,
                Tint = Color.Black,
                Parent = this,
                Size = Size,
            };

            Dim = dim;
            HasParallaxEffect = hasParallaxEffect;

            // Hook onto the event when the resolution changes, so the background's size can change
            // accordingly.
            WindowManager.ResolutionChanged += OnResolutionChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformParallaxEffect();
            base.Update(gameTime);
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

        /// <summary>
        ///     Performs a parallax effect on the background if specified.
        /// </summary>
        private void PerformParallaxEffect()
        {
            if (!HasParallaxEffect)
                return;

            // Parallax
            var mousePos = MouseManager.CurrentState.Position;

            Y = (mousePos.Y - WindowManager.VirtualScreen.Y / 2f) / 60f - 50;
            X = (mousePos.X - WindowManager.VirtualScreen.X / 2f) / 60f - 50;
        }
    }
}