namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     An italic style applied to a UTF-16 character range in a text sprite.
    /// </summary>
    public readonly struct TextItalicRange
    {
        public int StartIndex { get; }

        public int Length { get; }

        public TextItalicRange(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }
    }
}