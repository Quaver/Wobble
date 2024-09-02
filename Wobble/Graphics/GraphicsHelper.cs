using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Wobble.Graphics
{
    public static class GraphicsHelper
    {
        /// <summary>
        /// Returns a 1-dimensional value for an object's alignment within the provided boundary.
        /// </summary>
        /// <param name="scale">The value (percentage) which the object will be aligned to (0=min, 0.5 =mid, 1.0 = max)</param>
        /// <param name="objectSize">The size of the object</param>
        /// <param name="boundaryLeft"></param>
        /// <param name="boundaryRight"></param>
        /// <param name="offset"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        public static float Align(float scale, float objectSize, float boundaryLeft, float boundaryRight,
            float offset = 0, bool relative = false)
        {
            var res = (boundaryRight - boundaryLeft - objectSize) * scale + offset;
            return relative ? res : boundaryLeft + res;
        }

        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="objectAlignment">The alignment of the object.</param>
        /// <param name="objectRect">The size of the object.</param>
        /// <param name="boundary">The Rectangle of the boundary.</param>
        /// <param name="relative"></param>
        /// <returns></returns>
        public static RectangleF AlignRect(Alignment objectAlignment, RectangleF objectRect, RectangleF boundary,
            bool relative = false)
        {
            float alignX = 0;
            float alignY = 0;

            // Set the X-Alignment Scale
            switch (objectAlignment)
            {
                case Alignment.BotCenter:
                case Alignment.MidCenter:
                case Alignment.TopCenter:
                    alignX = 0.5f;
                    break;
                case Alignment.BotRight:
                case Alignment.MidRight:
                case Alignment.TopRight:
                    alignX = 1f;
                    break;
                default:
                    break;
            }

            // Set the Y-Alignment Scale
            switch (objectAlignment)
            {
                case Alignment.MidLeft:
                case Alignment.MidCenter:
                case Alignment.MidRight:
                    alignY = 0.5f;
                    break;
                case Alignment.BotLeft:
                case Alignment.BotCenter:
                case Alignment.BotRight:
                    alignY = 1f;
                    break;
                default:
                    break;
            }

            //Set X and Y Alignments
            alignX = Align(alignX, objectRect.Width, boundary.X, boundary.X + boundary.Width, objectRect.X, relative);
            alignY = Align(alignY, objectRect.Height, boundary.Y, boundary.Y + boundary.Height, objectRect.Y, relative);

            return new RectangleF(alignX, alignY, objectRect.Width, objectRect.Height);
        }

        public static RectangleF Offset(RectangleF objectRect, RectangleF offset)
        {
            return new RectangleF(objectRect.X + offset.X, objectRect.Y + offset.Y,
                objectRect.Width, objectRect.Height);
        }

        public static RectangleF Transform(RectangleF objectRect, Matrix2 matrix, Vector2 scale)
        {
            var resultPosition = matrix.Transform(objectRect.Position);
            var resultSize = new Size2(objectRect.Width * scale.X, objectRect.Height * scale.Y);
            return new RectangleF(resultPosition, resultSize);
        }

        public static RectangleF MinimumBoundingRectangle(RectangleF objectRect, float angleRadians,
            bool relative = false)
        {
            var cos = MathF.Cos(angleRadians);
            var sin = MathF.Sin(angleRadians);
            var topLeft = Vector2.Zero;
            var bottomLeft = new Vector2(-sin * objectRect.Height, cos * objectRect.Height);
            var bottomRight = new Vector2(cos * objectRect.Width - sin * objectRect.Height,
                sin * objectRect.Width + cos * objectRect.Height);
            var topRight = new Vector2(cos * objectRect.Width, sin * objectRect.Width);
            var minimumBoundingRectangle = MinimumBoundingRectangle(topLeft, topRight, bottomLeft, bottomRight);
            if (!relative)
                minimumBoundingRectangle.Offset(objectRect.Position);
            return minimumBoundingRectangle;
        }

        public static RectangleF MinimumBoundingRectangle(
            Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            var minX = MathF.Min(MathF.Min(topLeft.X, bottomLeft.X), MathF.Min(bottomRight.X, topRight.X));
            var minY = MathF.Min(MathF.Min(topLeft.Y, bottomLeft.Y), MathF.Min(bottomRight.Y, topRight.Y));
            var maxX = MathF.Max(MathF.Max(topLeft.X, bottomLeft.X), MathF.Max(bottomRight.X, topRight.X));
            var maxY = MathF.Max(MathF.Max(topLeft.Y, bottomLeft.Y), MathF.Max(bottomRight.Y, topRight.Y));
            var minimumBoundingRectangle = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            return minimumBoundingRectangle;
        }

        public static RectangleF MinimumBoundingRectangle(
            Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight)
        {
            var minX = MathF.Min(MathF.Min(topLeft.X, bottomLeft.X), MathF.Min(bottomRight.X, topRight.X));
            var minY = MathF.Min(MathF.Min(topLeft.Y, bottomLeft.Y), MathF.Min(bottomRight.Y, topRight.Y));
            var maxX = MathF.Max(MathF.Max(topLeft.X, bottomLeft.X), MathF.Max(bottomRight.X, topRight.X));
            var maxY = MathF.Max(MathF.Max(topLeft.Y, bottomLeft.Y), MathF.Max(bottomRight.Y, topRight.Y));
            var minimumBoundingRectangle = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            return minimumBoundingRectangle;
        }

        /// <summary>
        ///     Converts a Vector2 to Point
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Point Vector2ToPoint(Vector2 vector2) => new Point((int)vector2.X, (int)vector2.Y);

        /// <summary>
        ///     Converts a Point to Vector2
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal static Vector2 PointToVector2(Point point) => new Vector2(point.X, point.Y);

        /// <summary>
        ///      Check if a Vector2 point is inside a DrawRectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool RectangleContains(DrawRectangle rect, Vector2 point)
        {
            return (point.X >= rect.X && point.X <= rect.X + rect.Width && point.Y >= rect.Y &&
                    point.Y <= rect.Y + rect.Height);
        }

        public static bool RectangleContains(RectangleF rect, Vector2 point)
        {
            return (point.X >= rect.X && point.X <= rect.X + rect.Width && point.Y >= rect.Y &&
                    point.Y <= rect.Y + rect.Height);
        }
    }
}