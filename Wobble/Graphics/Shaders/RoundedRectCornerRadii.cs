using System;

namespace Wobble.Graphics.Shaders
{
    /// <summary>
    ///     Corner radii for a rounded rectangle, ordered clockwise from the top-left corner.
    /// </summary>
    public readonly struct RoundedRectCornerRadii : IEquatable<RoundedRectCornerRadii>
    {
        public float TopLeft { get; }

        public float TopRight { get; }

        public float BottomRight { get; }

        public float BottomLeft { get; }

        public RoundedRectCornerRadii(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public bool Equals(RoundedRectCornerRadii other) =>
            TopLeft.Equals(other.TopLeft) && TopRight.Equals(other.TopRight) &&
            BottomRight.Equals(other.BottomRight) && BottomLeft.Equals(other.BottomLeft);

        public override bool Equals(object obj) =>
            obj is RoundedRectCornerRadii other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TopLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ TopRight.GetHashCode();
                hashCode = (hashCode * 397) ^ BottomRight.GetHashCode();
                return (hashCode * 397) ^ BottomLeft.GetHashCode();
            }
        }

        public static RoundedRectCornerRadii All(float radius) =>
            new RoundedRectCornerRadii(radius, radius, radius, radius);
    }
}
