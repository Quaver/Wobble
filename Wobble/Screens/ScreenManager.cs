using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Graphics.UI.Buttons;

namespace Wobble.Screens
{
    public static class ScreenManager
    {
        /// <summary>
        ///     The screen that is currently being drawn
        /// </summary>
        private static Screen CurrentScreen { get; set; }

        /// <summary>
        ///     The screen that is queued to be changed.
        /// </summary>
        private static Screen QueuedScreen { get; set; }

        private static object LockObject { get; } = new object();

        /// <summary>
        ///     Removes all screens and places this one in the stack.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="switchImmediately"></param>
        public static void ChangeScreen(Screen screen, bool switchImmediately = false)
        {;
            lock (LockObject)
            {
                if (switchImmediately)
                {
                    CurrentScreen?.Destroy();
                    CurrentScreen = screen;
                    QueuedScreen = null;
                    return;
                }

                QueuedScreen = screen;
            }
        }

        /// <summary>
        ///     Updates the current screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);

            if (QueuedScreen == null)
                return;

            // Switch to queued screen after last update.
            CurrentScreen?.Destroy();
            CurrentScreen = QueuedScreen;
            QueuedScreen = null;
        }

        /// <summary>
        ///     Draws the current screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime) => CurrentScreen?.Draw(gameTime);
    }
}
