using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Wobble.Graphics.UI
{
    public class Cursor : Sprite
    {
        /// <summary>
        ///     The original size of the cursor; set during initialization.
        /// </summary>
        public sbyte OriginalSize { get; }

        /// <summary>
        ///     The scale at which the cursor's size will expand when the mouse
        ///     button is down.
        /// </summary>
        public float ExpandScale { get; set; }

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
            if (MouseManager.CurrentState.LeftButton == ButtonState.Pressed)
            {
                // Calculate the new size that the cursor will be when pressed.
                var newSize = MathHelper.Lerp(Width, OriginalSize * ExpandScale, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));
                Size = new ScalableVector2(newSize, newSize);
            }
            else
            {
                // Calculate new size when not pressed.
                var newSize = MathHelper.Lerp(Width, OriginalSize, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));
                Size = new ScalableVector2(newSize, newSize);
            }

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
            AnimationCompletionTime = time;
            CurrentAnimationTime = 0;
        }

        /// <summary>
        ///     Hides the cursor in a given amount of time.
        /// </summary>
        /// <param name="time"></param>
        public void Hide(int time)
        {
            IsShown = false;
            AnimationCompletionTime = time;
            CurrentAnimationTime = 0;
        }

        private void PerformShowAndHideAnimations(GameTime gameTime)
        {
            CurrentAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            var lerpTime = CurrentAnimationTime / AnimationCompletionTime;

            Alpha = MathHelper.Lerp(Alpha, IsShown ? 1 : 0, (float)lerpTime);
        }
    }
}
