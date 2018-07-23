using Microsoft.Xna.Framework;
using Wobble.Graphics;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Wobble.Screens
{
    public abstract class Screen : IDrawable
    {
        /// <summary>
        ///     The drawable user interface for the screen.
        /// </summary>
        public abstract ScreenInterface Interface { get; protected set; }

         /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime) => Interface.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime) => Interface.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => Interface.Destroy();
    }
}