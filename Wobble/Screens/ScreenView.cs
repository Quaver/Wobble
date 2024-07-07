using Microsoft.Xna.Framework;
using Wobble.Graphics;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Wobble.Screens
{
    /// <summary>
    ///     This object contains the UI for each screen. This needs to be implemented per-screen.
    /// </summary>
    public abstract class ScreenView : IDrawable
    {
        /// <summary>
        ///     Reference to the screen this UI is for.
        /// </summary>
        public Screen Screen { get; }

        /// <summary>
        ///     The container for all normal sprites.
        /// </summary>
        public Container Container { get; } = new Container();

        /// <summary>
        ///     The color to clear
        /// </summary>
        public virtual Color? ClearColor { get; } = null;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public ScreenView(Screen screen) => Screen = screen;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Draw(GameTime gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public abstract void Destroy();
    }
}