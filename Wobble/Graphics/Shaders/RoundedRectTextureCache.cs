using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Shaders
{
    /// <summary>
    ///     Creates exact-size rounded rectangle textures once, then shares them between buttons.
    ///     Keeping the rounding in the texture lets all button backgrounds, labels, and icons use
    ///     the same SpriteBatch instead of forcing a shader batch break for every button.
    /// </summary>
    internal static class RoundedRectTextureCache
    {
        private static Dictionary<TextureKey, Texture2D> Textures { get; } = new Dictionary<TextureKey, Texture2D>();

        public static Texture2D Get(float width, float height, float radius, bool antiAliased = true)
        {
            var textureWidth = Math.Max(1, (int) Math.Ceiling(width));
            var textureHeight = Math.Max(1, (int) Math.Ceiling(height));
            var scaledRadius = MathHelper.Clamp(
                radius * Math.Min(textureWidth / width, textureHeight / height),
                0,
                Math.Min(textureWidth, textureHeight) / 2f);
            var key = new TextureKey(textureWidth, textureHeight, scaledRadius, antiAliased);

            if (Textures.TryGetValue(key, out var texture) && !texture.IsDisposed)
                return texture;

            texture = Create(textureWidth, textureHeight, scaledRadius, antiAliased);
            Textures[key] = texture;
            return texture;
        }

        private static Texture2D Create(int width, int height, float radius, bool antiAliased)
        {
            var texture = new Texture2D(GameBase.Game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            var pixels = new Color[width * height];
            var halfWidth = width / 2f;
            var halfHeight = height / 2f;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var qx = Math.Abs(x + 0.5f - halfWidth) - (halfWidth - radius);
                    var qy = Math.Abs(y + 0.5f - halfHeight) - (halfHeight - radius);
                    var outsideDistance = (float) Math.Sqrt(Math.Max(qx, 0) * Math.Max(qx, 0) +
                                                            Math.Max(qy, 0) * Math.Max(qy, 0));
                    var distance = outsideDistance + Math.Min(Math.Max(qx, qy), 0) - radius;

                    var coverage = antiAliased ? 1 - SmoothStep(-1, 0, distance) : distance < 0 ? 1 : 0;
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

            private int RadiusBits { get; }

            private bool AntiAliased { get; }

            public TextureKey(int width, int height, float radius, bool antiAliased)
            {
                Width = width;
                Height = height;
                RadiusBits = BitConverter.SingleToInt32Bits(radius);
                AntiAliased = antiAliased;
            }

            public bool Equals(TextureKey other) =>
                Width == other.Width && Height == other.Height && RadiusBits == other.RadiusBits &&
                AntiAliased == other.AntiAliased;

            public override bool Equals(object obj) => obj is TextureKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Width;
                    hashCode = (hashCode * 397) ^ Height;
                    hashCode = (hashCode * 397) ^ RadiusBits;
                    return (hashCode * 397) ^ AntiAliased.GetHashCode();
                }
            }
        }
    }
}
