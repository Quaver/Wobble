using Microsoft.Xna.Framework;
using Wobble.Graphics;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Wobble.Screens
{
    public abstract class Screen : IDrawable
    {
        /// <summary>
        ///     If set to true, the screen will be automatically destroyed when calling
        ///     <see cref="ScreenManager.RemoveScreen"/>. It would be useful to set this to false
        ///     if you want to keep the same instance of a screen around
        /// </summary>
        public bool AutomaticallyDestroyOnScreenSwitch { get; set; } = true;

        /// <summary>
        ///     The drawable user interface for the screen.
        /// </summary>
        public abstract ScreenView View { get; protected set; }

         /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime) => View.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime) => View.Draw(gameTime);

        /// <summary>
        ///     Clears the screen
        /// </summary>
        public void Clear()
        {
            if (View?.ClearColor == null)
                return;

            GameBase.Game.GraphicsDevice.Clear(View.ClearColor);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => View.Destroy();
    }
}