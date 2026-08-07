using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Window;

namespace Wobble.Input
{
    /// <summary>
    ///     An enhanced version of the mouse state. It takes into account window screen scale,
    ///     to provide the actual accurate position of the mouse.
    /// </summary>
    public struct EnhancedMouseState
    {
        /// <summary>
        ///     The state of the left mouse button
        /// </summary>
        public ButtonState LeftButton { get; }

        /// <summary>
        ///     The state of the right mouse button
        /// </summary>
        public ButtonState RightButton { get; }

        /// <summary>
        ///     The state of the middle mouse button
        /// </summary>
        public ButtonState MiddleButton { get; }

        /// <summary>
        ///     The state of the 1st thumb mouse button.
        /// </summary>
        public ButtonState XButton1 { get; }

        /// <summary>
        ///     The state of the 2nd thumb mouse button
        /// </summary>
        public ButtonState XButton2 { get; }

        /// <summary>
        ///     The cumulative scroll wheel value since the game has started
        /// </summary>
        public int ScrollWheelValue { get; }

        /// <summary>
        ///     The position of the mouse.
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        ///     The X position of the mouse
        /// </summary>
        public float X => Position.X;

        /// <summary>
        ///     The Y position of the mouse.
        /// </summary>
        public float Y => Position.Y;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="state"></param>
        public EnhancedMouseState(MouseState state)
        {
            LeftButton = state.LeftButton;
            RightButton = state.RightButton;
            MiddleButton = state.MiddleButton;
            XButton1 = state.XButton1;
            XButton2 = state.XButton2;
            ScrollWheelValue = state.ScrollWheelValue;
            Position = new Vector2(state.Position.X, state.Position.Y) / WindowManager.ScreenScale;
        }

        /// <summary>
        ///     Creates a mouse state from a logical window position and explicit button values.
        ///     This is used by deterministic UI automation and keeps the normal mouse state shape intact.
        /// </summary>
        public EnhancedMouseState(Vector2 position, int scrollWheelValue, ButtonState leftButton,
            ButtonState rightButton, ButtonState middleButton, ButtonState xButton1, ButtonState xButton2)
        {
            LeftButton = leftButton;
            RightButton = rightButton;
            MiddleButton = middleButton;
            XButton1 = xButton1;
            XButton2 = xButton2;
            ScrollWheelValue = scrollWheelValue;
            Position = position;
        }
    }
}
