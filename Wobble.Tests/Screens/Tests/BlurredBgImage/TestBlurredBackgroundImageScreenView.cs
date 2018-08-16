using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.BlurredBgImage
{
    public class TestBlurredBackgroundImageScreenView : ScreenView
    {
        public TestBlurredBackgroundImageScreenView(Screen screen) : base(screen)
        {
            var background = new BlurredBackgroundImage(WobbleAssets.Wallpaper, BlurType.Gaussian, 10, 10)
            {
                Parent = Container
            };
        }

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
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}