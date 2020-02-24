using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Input;
using Wobble.Window;

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

                var x = MathHelper.Clamp(MouseManager.CurrentState.X - GrabOffset.Value.X, 0, WindowManager.Width - Width);
                var y = MathHelper.Clamp(MouseManager.CurrentState.Y - GrabOffset.Value.Y, 0, WindowManager.Height - Height);

                Position = new ScalableVector2(x, y);
            }
            else
            {
                GrabOffset = null;
            }

            base.Update(gameTime);
        }
    }
}