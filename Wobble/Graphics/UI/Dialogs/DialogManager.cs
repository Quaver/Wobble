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
            for (var i = 0; i < Dialogs.Count; i++)
            {
                var dialog = Dialogs[i];

                // Update each dialog.
                dialog.Update(gameTime);

                // Only handle input for the last dialog in the stack.
                if (i == Dialogs.Count - 1)
                    dialog.HandleInput(gameTime);
            }

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
                    x.Layer = GameBase.Game.MainLayerManager.DialogLayer;
                    x.Update(gameTime);
                });

                DialogsToBeAdded = new List<DialogScreen>();
            }
        }

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
        ///     Dismisses a specific dialog screen
        /// </summary>
        /// <param name="screen"></param>
        public static void Dismiss(DialogScreen screen)
        {
            if (!Dialogs.Contains(screen))
                return;

            DialogsToBeRemoved = new List<DialogScreen>() {screen};
        }

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
