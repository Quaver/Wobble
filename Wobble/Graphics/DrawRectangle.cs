namespace Wobble.Graphics
{
    public class DrawRectangle
    {
        /// <summary>
        ///     Position X
        /// </summary>
        public float X { get; set; }

        /// <summary>
        ///     Position Y
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        ///     Width Size
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        ///     Height Size
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        ///     Create a Rectangle for drawable classes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public DrawRectangle(float x = 0, float y = 0, float width = 0, float height = 0)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}