using System;
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
        /// <param name="boundaryX"></param>
        /// <param name="boundaryY"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static float Align(float scale, float objectSize, float boundaryX, float boundaryY, float offset = 0)
        {
            return Math.Min(boundaryX, boundaryY) + (Math.Abs(boundaryX - boundaryY) - objectSize) * scale + offset;
        }

        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="objectAlignment">The alignment of the object.</param>
        /// <param name="objectRect">The size of the object.</param>
        /// <param name="boundary">The Rectangle of the boundary.</param>
        /// <returns></returns>
        internal static RectangleF AlignRect(Alignment objectAlignment, RectangleF objectRect, RectangleF boundary)
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
            alignX = Align(alignX, objectRect.Width, boundary.X, boundary.X + boundary.Width, objectRect.X);
            alignY = Align(alignY, objectRect.Height, boundary.Y, boundary.Y + boundary.Height, objectRect.Y);

            return new RectangleF(alignX, alignY, objectRect.Width, objectRect.Height);
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
            return (point.X >= rect.X && point.X <= rect.X + rect.Width && point.Y >= rect.Y && point.Y <= rect.Y + rect.Height);
        }

        public static bool RectangleContains(RectangleF rect, Vector2 point)
        {
            return (point.X >= rect.X && point.X <= rect.X + rect.Width && point.Y >= rect.Y && point.Y <= rect.Y + rect.Height);
        }
    }
}