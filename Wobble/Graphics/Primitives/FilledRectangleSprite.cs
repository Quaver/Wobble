using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.Primitives
{
    /// <summary>
    ///     A rectangle that is filled in.
    /// </summary>
    public class FilledRectangleSprite : Sprite
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            GameBase.Game.SpriteBatch.FillRectangle(new Vector2(RenderRectangle.X, RenderRectangle.Y),
                new Vector2(RenderRectangle.Width, RenderRectangle.Height), Tint * Alpha, Rotation);
        }
    }
}