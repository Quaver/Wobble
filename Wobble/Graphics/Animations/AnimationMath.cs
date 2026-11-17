using System;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Animations
{
    /// <summary>
    ///     Helpers for visual smoothing that must remain stable across different update rates.
    /// </summary>
    internal static class AnimationMath
    {
        /// <summary>
        ///     Returns the interpolation amount for exponential smoothing over an elapsed time.
        /// </summary>
        internal static float SmoothingAmount(double elapsedMilliseconds, float timeConstantMilliseconds)
        {
            if (elapsedMilliseconds <= 0)
                return 0;

            if (timeConstantMilliseconds <= 0)
                return 1;

            return (float) (1 - Math.Exp(-elapsedMilliseconds / timeConstantMilliseconds));
        }

        /// <summary>
        ///     Smooths a value towards a target independently of update frequency.
        /// </summary>
        internal static float Damp(float current, float target, double elapsedMilliseconds,
            float timeConstantMilliseconds) => MathHelper.Lerp(current, target,
            SmoothingAmount(elapsedMilliseconds, timeConstantMilliseconds));
    }
}
