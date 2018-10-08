using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.Primitives
{
    /// <summary>
    ///     A simple line.
    /// </summary>
    public class Line : Sprite
    {
        /// <summary>
        ///     The ending position of the line.
        /// </summary>
        public Vector2 EndPosition { get; set; }

        /// <summary>
        ///     The thickness of the line.
        /// </summary>
        public float Thickness { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        public Line(Vector2 endPosition, Color color, float thickness)
        {
            EndPosition = endPosition;
            Tint = color;
            Thickness = thickness;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            GameBase.Game.SpriteBatch.DrawLine(new Vector2(RenderRectangle.X, RenderRectangle.Y),
                EndPosition, Tint * Alpha, Thickness);
        }
    }
}
