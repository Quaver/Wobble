using System;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Transformations
{
    public class Transformation
    {
        /// <summary>
        ///     The properties of the drawable that will be changed.
        /// </summary>
        public TransformationProperty Properties { get; }

        /// <summary>
        ///     The type of easing function this transformation will perform.
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
        ///     The time to complete the transformation.
        /// </summary>
        public float Time { get; }

        /// <summary>
        ///     The current time it's taken to animate
        /// </summary>
        public double CurrentAnimationTime { get; private set; }

        /// <summary>
        ///     Dictates if the transformation is done.
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="easingType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        public Transformation(TransformationProperty properties, Easing easingType, float start, float end, float time)
        {
            Properties = properties;
            EasingType = easingType;
            Start = start;
            End = end;
            Time = time;
        }

        /// <summary>
        ///     Performs the interpolation function for the transformation 
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
    }
}
