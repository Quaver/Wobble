using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.BlurredBgImage
{
    public class TestBlurredBackgroundImageScreenView : ScreenView
    {
        public TestBlurredBackgroundImageScreenView(Screen screen) : base(screen)
        {
            var blur = new GaussianBlur(1.1f);
            var image = blur.PerformGaussianBlur(WobbleAssets.Wallpaper);

            var background = new BackgroundImage(image)
            {
                Parent = Container,
                Image = image
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