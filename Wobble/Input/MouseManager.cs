using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Wobble.Input
{
    public static class MouseManager
    {
        /// <summary>
        ///     The current mouse state.
        /// </summary>
        public static EnhancedMouseState CurrentState { get; private set; }

        /// <summary>
        ///     The previous mouse state.
        /// </summary>
        public static EnhancedMouseState PreviousState { get; private set; }

        /// <summary>
        ///     Updates the MouseManager and keeps track of the current and previous mouse states.
        /// </summary>
        public static void Update()
        {
            PreviousState = CurrentState;
            CurrentState = new EnhancedMouseState(Mouse.GetState());
        }

        /// <summary>
        ///     Returns if the given button was a unique click.
        ///     A unique click means that the mouse was pressed and then released.
        /// </summary>
        public static bool IsUniqueClick(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return CurrentState.LeftButton == ButtonState.Released  && PreviousState.LeftButton == ButtonState.Pressed; 
                case MouseButton.Right:
                    return CurrentState.RightButton == ButtonState.Released  && PreviousState.RightButton == ButtonState.Pressed; 
                case MouseButton.Middle:
                    return CurrentState.MiddleButton == ButtonState.Released  && PreviousState.MiddleButton == ButtonState.Pressed;
                case MouseButton.Thumb1:
                    return CurrentState.XButton1 == ButtonState.Released  && PreviousState.XButton1 == ButtonState.Pressed; 
                case MouseButton.Thumb2:
                    return CurrentState.XButton2== ButtonState.Released  && PreviousState.XButton2 == ButtonState.Pressed; 
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        /// <summary>
        ///     Shows the mouse cursor.
        /// </summary>
        public static void ShowCursor() => WobbleGame.Instance.IsMouseVisible = true;

        /// <summary>
        ///     Hides the mouse cursor.
        /// </summary>
        /// <param name="game"></param>
        public static void HideCursor() => WobbleGame.Instance.IsMouseVisible = false;
    }
}