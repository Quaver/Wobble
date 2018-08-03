using Microsoft.Xna.Framework;

namespace Wobble.Graphics.UI.Dialogs
{
    public static class DialogManager
    {
        /// <summary>
        ///     The dialogScreen that is currently shown.
        /// </summary>
        public static DialogScreen CurrentDialogScreen { get; private set; }

        /// <summary>
        ///     Updates the current dialogScreen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime) => CurrentDialogScreen?.Update(gameTime);

        /// <summary>
        ///     Draws the current dialogScreen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime) => CurrentDialogScreen?.Draw(gameTime);

        /// <summary>
        ///     Shows a dialogScreen on the screen.
        /// </summary>
        /// <param name="dialogScreen"></param>
        public static void Show(DialogScreen dialogScreen)
        {
            if (CurrentDialogScreen != null)
                Dismiss();

            CurrentDialogScreen = dialogScreen;
            CurrentDialogScreen.Parent = null;
        }

        /// <summary>
        ///     Dismisses the current dialogScreen.
        /// </summary>
        public static void Dismiss()
        {
            CurrentDialogScreen?.Destroy();
            CurrentDialogScreen = null;
        }
    }
}
