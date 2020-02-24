using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Input;

namespace Wobble.Graphics.UI.Buttons
{
    public class DraggableButton : ImageButton
    {
        private Vector2? GrabOffset { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="clickAction"></param>
        public DraggableButton(Texture2D image, EventHandler clickAction = null) : base(image, clickAction)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (IsHeld)
            {
                if (GrabOffset == null)
                    GrabOffset = new Vector2(MouseManager.CurrentState.X - AbsolutePosition.X, MouseManager.CurrentState.Y - AbsolutePosition.Y);

                Alignment = Alignment.TopLeft;

                Position = new ScalableVector2(MouseManager.CurrentState.X - GrabOffset.Value.X, MouseManager.CurrentState.Y - GrabOffset.Value.Y);
            }
            else
            {
                GrabOffset = null;
            }

            base.Update(gameTime);
        }
    }
}