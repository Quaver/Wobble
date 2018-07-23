using Microsoft.Xna.Framework;
using Wobble.Graphics;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Wobble.Screens
{
    public abstract class Screen : IDrawable
    {
        /// <summary>
        ///     The container for the screen.
        /// </summary>
        public Container Container { get; } = new Container();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime) => Container.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        ///     Abstract draw class. When using this, you'll need to implement drawing for the container.
        ///     This can vary depending on if you're using shaders or have any other logic. So it is
        ///     best to implement it per-screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Draw(GameTime gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => Container.Destroy();
    }
}