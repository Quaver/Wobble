using Microsoft.Xna.Framework;
using Wobble.Graphics;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Wobble.Screens
{
    public abstract class Screen : IDrawable
    {
        /// <summary>
        ///     Whether the screen view should be destroyed when another screen becomes current.
        ///     Set this to false only when the caller keeps and later reuses the screen instance.
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
        ///     Called after this screen becomes current and retained elements have been attached
        ///     to its view container.
        /// </summary>
        public virtual void OnActivated()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => View.Destroy();
    }
}
