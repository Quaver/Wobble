using Wobble.Window;

namespace Wobble.Graphics
{
    /// <inheritdoc />
    /// <summary>
    ///     Container as a parent for sprites to easily lay them out.
    ///     Default size is the virtual screen resolution.
    /// </summary>
    public class Container : Drawable
    {
        public Container()
        {
            Size = new ScalableVector2(WindowManager.Rectangle.Width, WindowManager.Rectangle.Height);
            Position = new ScalableVector2(0, 0);
        }

        public Container(ScalableVector2 size, ScalableVector2 position)
        {
            Size = size;
            Position = position;
        }

        public Container(float x, float y, float width, float height)
        {
            Size = new ScalableVector2(width, height);
            Position = new ScalableVector2(x, y);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Simply a container. There's no need to draw it to spritebatch.
        /// </summary>
        public override void DrawToSpriteBatch()
        {
        }
    }
}