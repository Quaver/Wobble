using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Wobble.Graphics.UI.Buttons
{
    /// <summary>
    ///     All different types of buttons should inherit from this basic button class.
    /// </summary>
    public abstract class Button : Sprite
    {
        /// <summary>
        ///     Event invoked when the button is clicked.
        /// </summary>
        public event EventHandler Clicked;

        /// <summary>
        ///     Returns true if the mouse is truly hovering over the button
        ///     and this button is the top layered one.
        /// </summary>
        public bool IsHovered { get; private set; }

        /// <summary>
        ///     Keeps track of if the button is just hovered;no matter the draw order.
        ///     This will allow us to figure out which button is on top by 
        /// </summary>
        private bool IsHoveredWithoutDrawOrder { get; set; }

        /// <summary>
        ///     If the button is held down and hovered over, it will
        ///     be in a state of waiting for the click release so that it
        ///     can perform the click action.
        /// </summary>
        internal bool WaitingForClickRelease { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="clickAction"></param>
        protected Button(EventHandler clickAction = null)
        {
            Clicked += clickAction;

            ButtonManager.Add(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Check if the mouse is in the click area, as well as if the game window is actually the active window.
            if (GameBase.Game.IsActive && Visible && IsMouseInClickArea())
            {
                // Set this to be hovered without the draw order check.
                IsHoveredWithoutDrawOrder = true;

                // Get the button that is on the top layer.
                var topLayerButton = ButtonManager.Buttons.FindAll(x => x.IsHoveredWithoutDrawOrder).OrderByDescending(x => x.DrawOrder).First();

                // Set this to be truly hovered and follow up with click actions for this button.
                if (topLayerButton == this)
                {
                    IsHovered = true;
                    OnHover(gameTime);

                    // If we're not waiting for a click reelase and the mouse button is currently held down,
                    // then we'll set this to true.
                    if (!WaitingForClickRelease && MouseManager.CurrentState.LeftButton == ButtonState.Pressed)
                    {
                        WaitingForClickRelease = true;
                    }
                    // In the event that we are waiting for a click release, and the user doesn, then we can call
                    // the click action.
                    else if (WaitingForClickRelease && MouseManager.CurrentState.LeftButton == ButtonState.Released)
                    {
                        Clicked?.Invoke(this, new EventArgs());

                        // Now that the button is clicked, reset the waiting property.
                        WaitingForClickRelease = false;
                    }
                }
                else
                {
                    IsHovered = false;
                    WaitingForClickRelease = false;

                    OnNotHovered(gameTime);
                }
            }
            else
            {
                IsHoveredWithoutDrawOrder = false;
                WaitingForClickRelease = false;

                OnNotHovered(gameTime);
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Clicked = null;
            ButtonManager.Remove(this);

            base.Destroy();
        }

        /// <summary>
        ///     Checks if the mouse is in the click area of the button.
        /// 
        ///     This is marked as virtual because some buttons may want to increase/decrease
        ///     their click area.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsMouseInClickArea() => GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position);

        /// <summary>
        ///     When the button is hovered over, this'll be called.
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void OnHover(GameTime gameTime) {}

        /// <summary>
        ///     When the button is not hovered, this'll be called.
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void OnNotHovered(GameTime gameTime) {}
    }
}
