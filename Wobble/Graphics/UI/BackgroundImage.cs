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
        ///      This is to keep the image centered; calculated when the background is resized.
        /// </summary>
        private Vector2 Offset { get; set; }

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
        private bool _hasParallaxEffect;

        public bool HasParallaxEffect
        {
            get => _hasParallaxEffect;
            set
            {
                _hasParallaxEffect = value;
                AutoResize();
            }
        }

        public new Texture2D Image
        {
            get => base.Image;
            set
            {
                base.Image = value;
                AutoResize();
            }
        }

    /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image">The background image to use.</param>
        /// <param name="hasParallaxEffect">If the background will move when the mouse cursor does.</param>
        /// <param name="dim">The background dim as a percentage from 1-100%</param>
        public BackgroundImage(Texture2D image, int dim = 0, bool hasParallaxEffect = true)
        {
            Image = image;
            HasParallaxEffect = hasParallaxEffect;

            BrightnessSprite = new Sprite
            {
                Image = WobbleAssets.WhiteBox,
                Tint = Color.Black,
                Parent = this,
                Size = Size,
            };

            Dim = dim;

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
        ///     Scale the background sprite to the virtual window.
        ///     Depends on HasParallaxEffect being properly initialized.
        /// </summary>
        private void AutoResize()
        {
            // az: In the future, maybe override HasParallaxEffect so this is set up automatically
            // additionally, override the image property, as the editor modifies Image after
            // the size of the image has already been fixed.
            // let's fit this into the virtual screen box across the smallest dimension.
            var ratioX = Image.Height / WindowManager.VirtualScreen.Y;
            var ratioY = Image.Width / WindowManager.VirtualScreen.X;

            // what dimension is this image smaller in? scale against that
            var scaleRatio = 1 / Math.Min(ratioX, ratioY);

            var delta = (HasParallaxEffect ? 820.0f / 720.0f : 1.0f);

            var width = Image.Width * scaleRatio * delta;
            var height = Image.Height * scaleRatio * delta;
            Size = new ScalableVector2(width, height);

            // "crop off" the excess
            Offset = new Vector2(-(width  - WindowManager.VirtualScreen.X) / 2.0f,
                                 -(height - WindowManager.VirtualScreen.Y) / 2.0f);


            X = Offset.X;
            Y = Offset.Y;

            // Given this can be called at construction time, we must check null.
            if (BrightnessSprite != null)
                BrightnessSprite.Size = Size;
        }

        /// <summary>
        ///     When the resolution of the game changes, we'll want to change the size of the background
        ///     as well to fit the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResolutionChanged(object sender, EventArgs e)
        {
            AutoResize();
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

            Y = (mousePos.Y - WindowManager.VirtualScreen.Y / 2f) / 60f;
            X = (mousePos.X - WindowManager.VirtualScreen.X / 2f) / 60f;

            Y += Offset.Y;
            X += Offset.X;
        }
    }
}