using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Wobble.Input
{
    public static class KeyboardManager
    {
        /// <summary>
        ///     The current keyboard state
        /// </summary>
        public static KeyboardState CurrentState { get; private set; }

        /// <summary>
        ///     The keyboard state of the previous frame
        /// </summary>
        public static KeyboardState PreviousState { get; private set; }

        /// <summary>
        ///     Keeps our keyboard states updated each frame
        /// </summary>
        public static void Update()
        {
            PreviousState = CurrentState;
            CurrentState = Keyboard.GetState();
        }

        /// <summary>
        ///     If the key was pressed and released - useful for actions that require a single key press.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsUniqueKeyPress(Keys key) => PreviousState.IsKeyDown(key) && CurrentState.IsKeyUp(key);
    }
}
