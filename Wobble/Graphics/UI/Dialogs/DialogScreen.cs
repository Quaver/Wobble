using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Wobble.Graphics.UI.Dialogs
{
    /// <inheritdoc />
    /// <summary>
    ///     This inherits from button because we want to make all buttons under it unclickable.
    /// </summary>
    public abstract class DialogScreen : Button
    {
        /// <summary>
        ///     Contains the content for the dialogScreen box.
        /// </summary>
        public Container Container { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="backgroundAlpha"></param>
        protected DialogScreen(float backgroundAlpha)
        {
            if (backgroundAlpha < 0 || backgroundAlpha > 1)
                throw new ArgumentException("backgroundAlpha must be between 0 and 1");

            // Turn the alpha all the way down so it's invisible.
            Alpha = backgroundAlpha;
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            // Create the actual background that will dim the screen.
            Tint = Color.Black;

            Container = new Container() { Parent = this };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the dialogScreen box content.
        /// </summary>
        public abstract void CreateContent();

        /// <summary>
        ///     Handles input for the dialogScreen box.
        /// </summary>
        public abstract void HandleInput(GameTime gameTime);

        /// <summary>
        ///     Dictates if this dialog screen is on top
        /// </summary>
        public bool IsOnTop => DialogManager.Dialogs.Count > 0 && DialogManager.Dialogs.Last() == this;

        /// <summary>
        ///     Dictates if this dialog screen is currently active
        /// </summary>
        public bool Active => DialogManager.Dialogs.Contains(this);
    }
}
