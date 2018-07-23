using Microsoft.Xna.Framework;

namespace Wobble.Graphics
{
    public interface IDrawable
    {
        /// <summary>
        ///     Called every frame to update the object.
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(GameTime gameTime);

        /// <summary>
        ///     Called every frame to draw the object.
        /// </summary>
        /// <param name="gameTime"></param>
        void Draw(GameTime gameTime);

        /// <summary>
        ///     Destroys the object.
        /// </summary>
        void Destroy();
    }
}