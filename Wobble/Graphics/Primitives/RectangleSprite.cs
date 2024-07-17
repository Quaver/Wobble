using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.Primitives
{
    /// <summary>
    ///     A rectangle that isn't filled in.
    /// </summary>
    public class RectangleSprite : Sprite
    {
        /// <summary>
        ///     How thick the rectangle border is.
        /// </summary>
        public float Thickness { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="thickness"></param>
        public RectangleSprite(float thickness = 1) => Thickness = thickness;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            GameBase.Game.SpriteBatch.DrawRectangle(new Vector2(RenderRectangle.X, RenderRectangle.Y),
                new Vector2(RenderRectangle.Width, RenderRectangle.Height), _color, Thickness);
        }
    }
}