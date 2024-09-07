using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.Primitives
{
    public class PrimitiveLineBatch : Sprite
    {
        /// <summary>
        ///     The list of points to be drawn.
        /// </summary>
        public List<Vector2> Points { get; set; }

        /// <summary>
        ///     The thickness of the lines.
        /// </summary>
        public float Thickness { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="points"></param>
        /// <param name="thickness"></param>
        public PrimitiveLineBatch(List<Vector2> points, float thickness = 1)
        {
            Points = points;
            Thickness = thickness;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            var transform = Transform.SelfWorldMatrix.Matrix;
            Primitives2D.DrawPoints(GameBase.Game.SpriteBatch, Points, ref transform, AbsoluteColor, Thickness);
        }
    }
}