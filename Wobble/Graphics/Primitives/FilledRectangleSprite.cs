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

            var worldMatrix = Transform.SelfWorldMatrix.Matrix;
            GameBase.Game.SpriteBatch.FillRectangle(
                RelativeRectangle.Size, ref worldMatrix, AbsoluteColor);
        }
    }
}