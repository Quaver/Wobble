using System;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Animations
{
    public class Animation
    {
        /// <summary>
        ///     The properties of the drawable that will be changed.
        /// </summary>
        public AnimationProperty Properties { get; }

        /// <summary>
        ///     The type of easing function this animation will perform.
        /// </summary>
        public Easing EasingType { get; }

        /// <summary>
        ///     The starting value of the property.
        /// </summary>
        public float Start { get; }

        /// <summary>
        ///     The ending value of the property
        /// </summary>
        public float End { get; }

        /// <summary>
        ///     The time to complete the animation.
        /// </summary>
        public float Time { get; }

        /// <summary>
        ///     The current time it's taken to animate
        /// </summary>
        public double CurrentAnimationTime { get; private set; }

        /// <summary>
        ///     Dictates if the animation is done.
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        ///     If doing a animation with color, it'll fade from this color.
        /// </summary>
        public Color StartColor { get; set; }

        /// <summary>
        ///     If doing a animation with color, it'll fade to this color
        /// </summary>
        public Color EndColor { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="easingType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        public Animation(AnimationProperty properties, Easing easingType, float start, float end, float time)
        {
            Properties = properties;
            EasingType = easingType;
            Start = start;
            End = end;
            Time = time;
        }

        /// <summary>
        /// </summary>
        /// <param name="easingType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        public Animation(Easing easingType, Color start, Color end, float time)
        {
            Properties = AnimationProperty.Color;
            EasingType = easingType;
            StartColor = start;
            EndColor = end;
            Time = time;
        }

        /// <summary>
        ///     Performs the interpolation function
        /// </summary>
        /// <param name="gameTime"></param>
        public float PerformInterpolation(GameTime gameTime)
        {
            CurrentAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentAnimationTime > Time)
                CurrentAnimationTime = Time;

            var val = EasingFunctions.Perform(EasingType, Start, End, (float)(CurrentAnimationTime / Time));

            if (Math.Abs(val - End) < 0.01)
                Done = true;

            return val;
        }

        /// <summary>
        ///     Performs interpolation with colors.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public Color PerformColorInterpolation(GameTime gameTime)
        {
            CurrentAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentAnimationTime > Time)
                CurrentAnimationTime = Time;

            var r = EasingFunctions.Perform(EasingType, StartColor.R, EndColor.R, (float) (CurrentAnimationTime / Time));
            var g = EasingFunctions.Perform(EasingType, StartColor.G, EndColor.G, (float) (CurrentAnimationTime / Time));
            var b = EasingFunctions.Perform(EasingType, StartColor.B, EndColor.B, (float) (CurrentAnimationTime / Time));

            if (Math.Abs(r - EndColor.R) < 0.01 && Math.Abs(g - EndColor.G) < 0.01 && Math.Abs(b - EndColor.B) < 0.01)
                Done = true;

            return new Color((int) r, (int) g, (int) b);
        }
    }
}
