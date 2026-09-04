namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     A bold style applied to a UTF-16 character range in a text sprite.
    /// </summary>
    public readonly struct TextBoldRange
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
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        public TextBoldRange(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }
    }
}