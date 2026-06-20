using System;
using Microsoft.Xna.Framework;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     Shared physical-pixel rules for text rasterization and placement.
    /// </summary>
    public static class TextRenderQuality
    {
        /// <summary>
        ///     Rasterize text at least at this scale. The default of 1 keeps cached text at
        ///     its native physical resolution and avoids softening from unconditional downsampling.
        ///     Applications can opt into supersampling by increasing this value.
        /// </summary>
        public static float MinimumRasterizationScale { get; set; } = 1f;

        public static Vector2 DisplayScale
        {
            get
            {
                var scale = WindowManager.ScreenScale;
                return new Vector2(ValidScale(scale.X), ValidScale(scale.Y));
            }
        }

        public static float RasterizationScale
        {
            get
            {
                var displayScale = DisplayScale;
                return Math.Max(MinimumRasterizationScale, Math.Max(displayScale.X, displayScale.Y));
            }
        }

        /// <summary>
        ///     Exact per-axis cache scale. Keeping X and Y separate avoids resampling cached text
        ///     when a window's aspect ratio does not match the virtual resolution.
        /// </summary>
        public static Vector2 CacheScale
        {
            get
            {
                var displayScale = DisplayScale;
                var maximumDisplayScale = Math.Max(displayScale.X, displayScale.Y);
                var supersampling = maximumDisplayScale <= 0
                    ? 1f
                    : Math.Max(1f, MinimumRasterizationScale / maximumDisplayScale);
                return displayScale * supersampling;
            }
        }

        public static float MaximumDisplayScale
        {
            get
            {
                var scale = DisplayScale;
                return Math.Max(scale.X, scale.Y);
            }
        }

        public static float Snap(float logicalPosition, float displayScale) =>
            (float)Math.Round(logicalPosition * ValidScale(displayScale), MidpointRounding.AwayFromZero) /
            ValidScale(displayScale);

        private static float ValidScale(float scale) =>
            float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0 ? 1f : scale;
    }
}
