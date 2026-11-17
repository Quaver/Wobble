using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Wobble.Graphics.UI
{
    public class Cursor : Sprite
    {
        public const float MinimumSizeScale = 0.1f;
        
        public const float MaximumSizeScale = 2.0f;
        
        /// <summary>
        ///     The original size of the cursor; set during initialization.
        /// </summary>
        public sbyte OriginalSize { get; }

        /// <summary>
        ///     The scale at which the cursor's size will expand when the mouse
        ///     button is down.
        /// </summary>
        public float ExpandScale { get; set; }
        
        private float _sizeScale = 1f;
        
        public float SizeScale
        {
            get => _sizeScale;
            set
            {
                _sizeScale = MathHelper.Clamp(value, MinimumSizeScale, MaximumSizeScale);
                var size = OriginalSize * _sizeScale;
                Size = new ScalableVector2(size, size);
            }
        }

        /// <summary>
        ///     Whether to center the cursor image on the cursor position.
        /// </summary>
        public bool Center { get; set; } = false;

        /// <summary>
        ///     If the cursor is currently shown
        /// </summary>
        private bool IsShown { get; set; } = true;

        /// <summary>
        ///     The time it takes for the show/hide animation to complete
        /// </summary>
        private int AnimationCompletionTime { get; set; }

        /// <summary>
        ///     The total time the animation has been running.
        /// </summary>
        private double CurrentAnimationTime { get; set; }

        private float AnimationStartAlpha { get; set; }

        private bool IsVisibilityAnimationActive { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        /// <param name="expandScale"></param>
        public Cursor(Texture2D image, sbyte size, float expandScale = 1.2f)
        {
            Size = new ScalableVector2(size, size);
            Image = image;
            OriginalSize = size;
            ExpandScale = expandScale;
            Show(1);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var baseSize = OriginalSize * SizeScale;
            var targetSize = MouseManager.CurrentState.LeftButton == ButtonState.Pressed
                ? baseSize * ExpandScale
                : baseSize;
            var newSize = AnimationMath.Damp(Width, targetSize,
                gameTime.ElapsedGameTime.TotalMilliseconds, 60);
            Size = new ScalableVector2(newSize, newSize);

            X = MouseManager.CurrentState.X;
            Y = MouseManager.CurrentState.Y;

            if (Center)
            {
                X -= Width / 2;
                Y -= Height / 2;
            }

            PerformShowAndHideAnimations(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Shows the cursor in a given amount of time
        /// </summary>
        /// <param name="time">The time to perform the animation in milliseconds.</param>
        public void Show(int time)
        {
            IsShown = true;
            StartVisibilityAnimation(time);
        }

        /// <summary>
        ///     Hides the cursor in a given amount of time.
        /// </summary>
        /// <param name="time"></param>
        public void Hide(int time)
        {
            IsShown = false;
            StartVisibilityAnimation(time);
        }

        private void PerformShowAndHideAnimations(GameTime gameTime)
        {
            if (!IsVisibilityAnimationActive)
                return;

            CurrentAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            var progress = MathHelper.Clamp((float) (CurrentAnimationTime / AnimationCompletionTime), 0, 1);

            Alpha = MathHelper.Lerp(AnimationStartAlpha, IsShown ? 1 : 0, progress);

            if (progress >= 1)
                IsVisibilityAnimationActive = false;
        }

        private void StartVisibilityAnimation(int time)
        {
            AnimationCompletionTime = Math.Max(0, time);
            CurrentAnimationTime = 0;
            AnimationStartAlpha = Alpha;

            if (AnimationCompletionTime == 0)
            {
                Alpha = IsShown ? 1 : 0;
                IsVisibilityAnimationActive = false;
                return;
            }

            IsVisibilityAnimationActive = true;
        }
    }
}
