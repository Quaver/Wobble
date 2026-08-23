using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Shaders
{
    /// <summary>
    ///     Creates rounded rectangle textures and shares nearby sizes between buttons. Cached textures
    ///     are weakly held so obsolete resize variants can be collected after drawables release them.
    ///     Keeping the rounding in the texture lets backgrounds, labels, and icons use the same
    ///     SpriteBatch instead of forcing a shader batch break for every button.
    /// </summary>
    public static class RoundedRectTextureCache
    {
        private const int TextureSizeBucket = 4;
        private const int ExactSizeThreshold = 64;
        private const int CleanupInterval = 64;
        private const float RadiusBucket = 0.125f;

        private static Dictionary<TextureKey, WeakReference<Texture2D>> Textures { get; } =
            new Dictionary<TextureKey, WeakReference<Texture2D>>();

        private static object SyncRoot { get; } = new object();

        private static int TexturesCreatedSinceCleanup { get; set; }

        public static Texture2D Get(float width, float height, float radius, bool antiAliased = true)
        {
            return Get(width, height, RoundedRectCornerRadii.All(radius), antiAliased);
        }

        public static Texture2D Get(float width, float height, RoundedRectCornerRadii radii,
            bool antiAliased = true)
        {
            lock (SyncRoot)
                return GetCached(width, height, radii, antiAliased);
        }

        private static Texture2D GetCached(float width, float height, RoundedRectCornerRadii radii,
            bool antiAliased)
        {
            var safeWidth = NormalizeDimension(width);
            var safeHeight = NormalizeDimension(height);
            var normalizedRadii = new RoundedRectCornerRadii(
                NormalizeRadius(radii.TopLeft),
                NormalizeRadius(radii.TopRight),
                NormalizeRadius(radii.BottomRight),
                NormalizeRadius(radii.BottomLeft));
            var requiresExactSize = HasAsymmetricRadii(normalizedRadii) ||
                                    Math.Min(safeWidth, safeHeight) <= ExactSizeThreshold;
            var textureWidth = requiresExactSize ? ExactDimension(safeWidth) : BucketDimension(safeWidth);
            var textureHeight = requiresExactSize ? ExactDimension(safeHeight) : BucketDimension(safeHeight);
            var dimensionScale = Math.Min(textureWidth / safeWidth, textureHeight / safeHeight);
            var maxRadius = Math.Min(textureWidth, textureHeight) / 2f;
            var largestRadius = Math.Max(
                Math.Max(normalizedRadii.TopLeft, normalizedRadii.TopRight),
                Math.Max(normalizedRadii.BottomRight, normalizedRadii.BottomLeft)) * dimensionScale;
            var radiusScale = largestRadius > maxRadius && largestRadius > 0
                ? maxRadius / largestRadius
                : 1f;
            var combinedScale = dimensionScale * radiusScale;
            var scaledRadii = new RoundedRectCornerRadii(
                BucketRadius(normalizedRadii.TopLeft * combinedScale),
                BucketRadius(normalizedRadii.TopRight * combinedScale),
                BucketRadius(normalizedRadii.BottomRight * combinedScale),
                BucketRadius(normalizedRadii.BottomLeft * combinedScale));
            var key = new TextureKey(textureWidth, textureHeight, scaledRadii, antiAliased);

            if (Textures.TryGetValue(key, out var reference) &&
                reference.TryGetTarget(out var texture) && !texture.IsDisposed)
                return texture;

            texture = Create(textureWidth, textureHeight, scaledRadii, antiAliased);
            Textures[key] = new WeakReference<Texture2D>(texture);

            if (++TexturesCreatedSinceCleanup >= CleanupInterval)
            {
                RemoveCollectedTextures();
                TexturesCreatedSinceCleanup = 0;
            }

            return texture;
        }

        private static float NormalizeDimension(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0 ? value : 1;

        private static float NormalizeRadius(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) ? Math.Max(0, value) : 0;

        private static bool HasAsymmetricRadii(RoundedRectCornerRadii radii) =>
            !radii.TopLeft.Equals(radii.TopRight) || !radii.TopRight.Equals(radii.BottomRight) ||
            !radii.BottomRight.Equals(radii.BottomLeft);

        private static int ExactDimension(float value) => Math.Max(1, (int) Math.Ceiling(value));

        private static int BucketDimension(float value)
        {
            var pixels = ExactDimension(value);
            return (pixels + TextureSizeBucket - 1) / TextureSizeBucket * TextureSizeBucket;
        }

        private static float BucketRadius(float value) =>
            (float) Math.Round(value / RadiusBucket) * RadiusBucket;

        private static void RemoveCollectedTextures()
        {
            var collectedKeys = new List<TextureKey>();
            foreach (var pair in Textures)
            {
                if (!pair.Value.TryGetTarget(out var texture) || texture.IsDisposed)
                    collectedKeys.Add(pair.Key);
            }

            foreach (var key in collectedKeys)
                Textures.Remove(key);
        }

        private static Texture2D Create(int width, int height, RoundedRectCornerRadii radii,
            bool antiAliased)
        {
            var texture = new Texture2D(GameBase.Game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            var pixels = new Color[width * height];
            var halfWidth = width / 2f;
            var halfHeight = height / 2f;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var radius = x < halfWidth
                        ? y < halfHeight ? radii.TopLeft : radii.BottomLeft
                        : y < halfHeight ? radii.TopRight : radii.BottomRight;
                    var qx = Math.Abs(x + 0.5f - halfWidth) - (halfWidth - radius);
                    var qy = Math.Abs(y + 0.5f - halfHeight) - (halfHeight - radius);
                    var outsideDistance = (float) Math.Sqrt(Math.Max(qx, 0) * Math.Max(qx, 0) +
                                                            Math.Max(qy, 0) * Math.Max(qy, 0));
                    var distance = outsideDistance + Math.Min(Math.Max(qx, qy), 0) - radius;

                    // Keep the AA transition centered on the mathematical edge. An entirely inward
                    // feather noticeably shrinks radii such as 3px and 6px, especially when different
                    // corners use different values.
                    var coverage = antiAliased
                        ? 1 - SmoothStep(-0.5f, 0.5f, distance)
                        : distance <= 0 ? 1 : 0;
                    pixels[y * width + x] = new Color((byte) 255, (byte) 255, (byte) 255, (byte) (coverage * 255));
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private static float SmoothStep(float min, float max, float value)
        {
            var amount = MathHelper.Clamp((value - min) / (max - min), 0, 1);
            return amount * amount * (3 - 2 * amount);
        }

        private readonly struct TextureKey : IEquatable<TextureKey>
        {
            private int Width { get; }

            private int Height { get; }

            private RoundedRectCornerRadii Radii { get; }

            private bool AntiAliased { get; }

            public TextureKey(int width, int height, RoundedRectCornerRadii radii, bool antiAliased)
            {
                Width = width;
                Height = height;
                Radii = radii;
                AntiAliased = antiAliased;
            }

            public bool Equals(TextureKey other) =>
                Width == other.Width && Height == other.Height && Radii.Equals(other.Radii) &&
                AntiAliased == other.AntiAliased;

            public override bool Equals(object obj) => obj is TextureKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Width;
                    hashCode = (hashCode * 397) ^ Height;
                    hashCode = (hashCode * 397) ^ Radii.GetHashCode();
                    return ((hashCode * 397) ^ AntiAliased.GetHashCode()) & int.MaxValue;
                }
            }
        }
    }
}
