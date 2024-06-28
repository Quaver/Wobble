using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics
{
    /// <summary>
    ///     Specifies the padding from four directions (up, down, left, right)
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Padding
    {
        [DataMember] public int Left;
        [DataMember] public int Right;
        [DataMember] public int Up;
        [DataMember] public int Down;

        public static readonly Padding Zero = new Padding(0, 0, 0, 0);

        public Padding(int left, int right, int up, int down)
        {
            Left = left;
            Right = right;
            Up = up;
            Down = down;
        }

        internal string DebugDisplayString => $"L: {Left} U: {Up} R: {Right} D: {Down}";

        /// <summary>
        ///     Displacement in position when padding *inwards*
        /// </summary>
        public Point Offset => new Point(Left, Up);

        /// <summary>
        ///     Increase in size when padding *outwards*
        /// </summary>
        /// <seealso cref="PadOutwards"/>
        public Point SizeIncrement => new Point(Left + Right, Up + Down);

        public static Rectangle operator -(Rectangle source, Padding padding)
        {
            return padding.PadInwards(source);
        }

        public static Rectangle operator +(Rectangle source, Padding padding)
        {
            return padding.PadOutwards(source);
        }

        /// <summary>
        ///     Extends the <see cref="sourceRectangle"/> in all four directions
        /// </summary>
        /// <param name="sourceRectangle"></param>
        /// <returns></returns>
        public Rectangle PadOutwards(Rectangle sourceRectangle) =>
            new Rectangle(sourceRectangle.Location - Offset, sourceRectangle.Size + SizeIncrement);

        /// <summary>
        ///     Shrinks the <see cref="sourceRectangle"/> in all four directions
        /// </summary>
        /// <param name="sourceRectangle"></param>
        /// <returns></returns>
        public Rectangle PadInwards(Rectangle sourceRectangle) =>
            new Rectangle(sourceRectangle.Location + Offset, sourceRectangle.Size - SizeIncrement);
    }
}