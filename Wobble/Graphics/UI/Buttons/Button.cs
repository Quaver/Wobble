using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
            }
            else
            {

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
    }
}
