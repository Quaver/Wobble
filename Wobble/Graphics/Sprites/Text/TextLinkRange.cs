using System;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     A link target applied to a UTF-16 character range in a text sprite.
    /// </summary>
    public readonly struct TextLinkRange
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
        ///     The target associated with the link. Consumers decide how the target is opened or routed.
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="target"></param>
        public TextLinkRange(int startIndex, int length, string target)
        {
            StartIndex = startIndex;
            Length = length;
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}