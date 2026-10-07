namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     A font size applied to a UTF-16 character range in a text sprite.
    /// </summary>
    public readonly struct TextFontSizeRange
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
        ///     The font size applied to the range.
        /// </summary>
        public float FontSize { get; }

        /// <summary>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="fontSize"></param>
        public TextFontSizeRange(int startIndex, int length, float fontSize)
        {
            StartIndex = startIndex;
            Length = length;
            FontSize = fontSize;
        }
    }
}