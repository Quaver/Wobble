using System;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Background
{
    public class TestBackgroundImageScreenView : ScreenView
    {
        /// <summary>
        ///     The background image to be displayed.
        /// </summary>
        private BackgroundImage Background { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestBackgroundImageScreenView(Screen screen) : base(screen) => Background = new BackgroundImage(WobbleAssets.WhiteBox, 60)
        {
            Parent = Container
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void Destroy() => Container?.Destroy();
    }
}
