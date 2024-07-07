using System;
using Microsoft.Xna.Framework;

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
        ///     Queues up a screen to be switched (or switches immediately).
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="switchImmediately"></param>
        public static void ChangeScreen(Screen screen, bool switchImmediately = false)
        {
            lock (LockObject)
            {
                if (switchImmediately)
                {
                    CurrentScreen?.Destroy();
                    CurrentScreen = screen;
                    QueuedScreen = null;
                    return;
                }

                QueuedScreen?.Destroy();
                QueuedScreen = screen;
            }
        }

        /// <summary>
        ///     Updates the current screen, and switches to a queued screen if one exists.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);

            if (QueuedScreen == null)
                return;

            // Switch to queued screen after last update.
            lock (LockObject)
            {
                CurrentScreen?.Destroy();
                CurrentScreen = QueuedScreen;
                QueuedScreen = null;
            }
        }

        public static void Clear() => CurrentScreen?.Clear();

        /// <summary>
        ///     Draws the current screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime) => CurrentScreen?.Draw(gameTime);
    }
}