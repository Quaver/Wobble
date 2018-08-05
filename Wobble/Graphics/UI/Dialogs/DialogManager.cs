using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.UI.Dialogs
{
    public static class DialogManager
    {
        /// <summary>
        ///     Stack of dialog screens. FIFO.
        /// </summary>
        public static List<DialogScreen> Dialogs { get; private set; } = new List<DialogScreen>();

        /// <summary>
        ///     Stores a list of dialogs to be removed.
        /// </summary>
        private static List<DialogScreen> DialogsToBeRemoved { get; set; } = new List<DialogScreen>();

        /// <summary>
        ///     Stores of the list of dialogs that need to be added.
        /// </summary>
        private static List<DialogScreen> DialogsToBeAdded { get; set; } = new List<DialogScreen>();

        /// <summary>
        ///     Updates the current dialogScreen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            Dialogs.ForEach(x => x.Update(gameTime));

            // Remove all dialogs that need to be removed at the end of this frame.
            if (DialogsToBeRemoved.Count > 0)
            {
                DialogsToBeRemoved.ForEach(x =>
                {
                    Dialogs.Remove(x);
                    x.Destroy();
                });

                DialogsToBeRemoved = new List<DialogScreen>();
            }

            // If we have any dialogs to add however, we'll want to add them after we remove
            // all of the disposed ones, and then update them accordingly.
            if (DialogsToBeAdded.Count > 0)
            {
                DialogsToBeAdded.ForEach(x =>
                {
                    Dialogs.Add(x);
                    x.Update(gameTime);
                });

                DialogsToBeAdded = new List<DialogScreen>();
            }
        }

        /// <summary>
        ///     Draws the current dialogScreen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime) => Dialogs.ForEach(x => x.Draw(gameTime));

        /// <summary>
        ///     Shows a dialogScreen on the screen.
        /// </summary>
        /// <param name="dialogScreen"></param>
        public static void Show(DialogScreen dialogScreen)
        {
            if (dialogScreen.Parent != null)
                throw new ArgumentException("Dialog must not have a parent!");

            DialogsToBeAdded.Add(dialogScreen);
        }

        /// <summary>
        ///     Dismisses the current dialogScreen.
        /// </summary>
        public static void Dismiss() => DialogsToBeRemoved = new List<DialogScreen> {Dialogs.Last()};

        /// <summary>
        ///     Dismisses all dialogs.
        /// </summary>
        public static void DismissAll()
        {
            DialogsToBeRemoved = new List<DialogScreen>();
            Dialogs.ForEach(x => Dialogs.Add(x));
        }
    }
}
