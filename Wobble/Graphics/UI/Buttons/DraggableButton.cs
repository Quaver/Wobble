using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Input;

namespace Wobble.Graphics.UI.Buttons
{
    public class DraggableButton : ImageButton
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="clickAction"></param>
        public DraggableButton(Texture2D image, EventHandler clickAction = null) : base(image, clickAction) { }

        /// <inheritdoc />
        /// <summary>
        ///     TODO: Fix this so that it works with every alignment.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnHeld(GameTime gameTime)
        {
            Alignment = Alignment.TopLeft;
            Position = new ScalableVector2(MouseManager.CurrentState.X, MouseManager.CurrentState.Y);
        }
    }
}