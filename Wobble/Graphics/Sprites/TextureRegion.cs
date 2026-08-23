using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites
{
    /// <summary>
    ///     A drawable region of a texture atlas.
    /// </summary>
    public readonly struct TextureRegion
    {
        public Texture2D Texture { get; }

        public Rectangle SourceRectangle { get; }

        public int Width => SourceRectangle.Width;

        public int Height => SourceRectangle.Height;

        public TextureRegion(Texture2D texture, Rectangle sourceRectangle)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (sourceRectangle.Width <= 0 || sourceRectangle.Height <= 0 ||
                !texture.Bounds.Contains(sourceRectangle))
            {
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), sourceRectangle,
                    "The texture region must be a non-empty rectangle inside the texture.");
            }

            Texture = texture;
            SourceRectangle = sourceRectangle;
        }

        public static TextureRegion From(Texture2D texture2D) =>
            new TextureRegion(texture2D, texture2D.Bounds);

        public static implicit operator TextureRegion(Texture2D texture2D) => From(texture2D);
    }
}
