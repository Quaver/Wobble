using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Wobble.Screens
{
    public static class ScreenManager
    {
        /// <summary>
        ///     Holds a stack of all the current game screens. It is setup in a first in, first out
        ///     way where the screen on top will be the one being updated/drawn, and will also be the
        ///     first to be removed.
        /// </summary>
        private static Stack<Screen> Screens { get; } = new Stack<Screen>();

        /// <summary>
        ///     Adds a screen to the stack. This screen will become the new main screen, but
        ///     the ones under it will still persist at the same state.
        /// </summary>
        /// <param name="screen"></param>
        public static void AddScreen(Screen screen) => Screens.Push(screen);

        /// <summary>
        ///     If there are currently any screens in the stack.
        /// </summary>
        public static bool HasScreens => Screens.Count > 0;

        /// <summary>
        ///     Removes the screen on top.
        /// </summary>
        public static void RemoveScreen(bool destroyInTask = false)
        {
            if (!HasScreens)
                return;

            // Destroy the screen. If specified to destroy in a task, it will do so.
            if (destroyInTask)
               Task.Run(() => Screens.Peek().Destroy());
            else
                Screens.Peek().Destroy();

            // Remove the screen.
            Screens.Pop();
        }

        /// <summary>
        ///     Removes every single screen that we have in the stack.
        /// </summary>
        public static void RemoveAllScreens()
        {
            while (HasScreens)
                RemoveScreen();
        }

        /// <summary>
        ///     Removes all screens and places this one in the stack.
        /// </summary>
        /// <param name="screen"></param>
        public static void ChangeScreen(Screen screen)
        {
            RemoveAllScreens();
            AddScreen(screen);
        }

        /// <summary>
        ///     Updates the current screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            if (!HasScreens)
                return;

            Screens.Peek().Update(gameTime);
        }

        /// <summary>
        ///     Draws the current screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime)
        {
            if (!HasScreens)
                return;

            Screens.Peek().Draw(gameTime);
        }

        /// <summary>
        ///     Asynchronously load a screen and call its initialize method.
        ///     After doing so, the callback action will be called
        /// </summary>
        public static void LoadAsync(Func<Screen> loadAction, Action callback) => Task.Run(loadAction).ContinueWith(t => callback());
    }
}
