using System;
using Microsoft.Xna.Framework;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI
{
    public class ProgressBar : Sprite
    {
        /// <summary>
        ///     The value the progress bar will be binded to.
        /// </summary>
        public BindableDouble Bindable { get; }

        /// <summary>
        ///     The original size of the progress bar.
        /// </summary>
        private Vector2 OriginalSize { get; }

        /// <summary>
        ///     The active progress bar that is overlayed on top of this one.
        /// </summary>
        private Sprite ActiveBar { get; set; }

        /// <summary>
        ///     The percentage of which the progress bar is at.
        /// </summary>
        private double Percentage => (Bindable.Value - 0) * 100 / Bindable.MaxValue - 0 * 100;

        /// <summary>
        ///     Dictates whether or not to destroy the bindable when the sprite is destroyed.
        /// </summary>
        public bool DisposeBindableOnDestroy { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="bindable"></param>
        /// <param name="inactiveColor"></param>
        /// <param name="activeColor"></param>
        /// <param name="disposeBindableOnDestroy">Dictates whether or not to destroy the bindable when the sprite is destroyed.</param>
        public ProgressBar(Vector2 size, BindableDouble bindable, Color inactiveColor, Color activeColor, bool disposeBindableOnDestroy)
        {
            Bindable = bindable;
            OriginalSize = size;
            DisposeBindableOnDestroy = disposeBindableOnDestroy;

            InitializeSprites(size, inactiveColor, activeColor);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="size"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="defaultValue"></param>
        /// <param name="inactiveColor"></param>
        /// <param name="activeColor"></param>
        public ProgressBar(Vector2 size, double minValue, double maxValue, double defaultValue, Color inactiveColor, Color activeColor)
        {
            Bindable = new BindableDouble(defaultValue, minValue, maxValue);
            OriginalSize = size;
            DisposeBindableOnDestroy = true;

            InitializeSprites(size, inactiveColor, activeColor);
        }

        /// <summary>
        ///     Initializes the sprites after doing initial construction.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="inactiveColor"></param>
        /// <param name="activeColor"></param>
        private void InitializeSprites(Vector2 size, Color inactiveColor, Color activeColor)
        {
            Size = new ScalableVector2(size.X, size.Y);
            Tint = inactiveColor;

            ActiveBar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(0, Height),
                Alignment = Alignment,
                Tint = activeColor
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // TODO : Add smoother easing function for this.
            ActiveBar.Width = MathHelper.LerpPrecise((float)(Width * (Percentage / 100f)), ActiveBar.Width, (float) Math.Min(dt / 240, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (DisposeBindableOnDestroy)
                Bindable.Dispose();

            base.Destroy();
        }
    }
}