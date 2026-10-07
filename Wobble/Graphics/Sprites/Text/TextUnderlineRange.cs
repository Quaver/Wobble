namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     An underline applied to a UTF-16 character range in a text sprite.
    ///     The underline uses the effective color of the text over the same range.
    /// </summary>
    public readonly struct TextUnderlineRange
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
        public TextUnderlineRange(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }
    }
}