using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     A custom color applied to a UTF-16 character range in a text sprite.
    /// </summary>
    public readonly struct TextColorRange
    {
        /// <summary>
        ///     The zero-based UTF-16 character index where the range begins.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        ///     The number of UTF-16 characters in the range.
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///     The color applied to the range.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="color"></param>
        public TextColorRange(int startIndex, int length, Color color)
        {
            StartIndex = startIndex;
            Length = length;
            Color = color;
        }
    }
}
