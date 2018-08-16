using System;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.Blur
{
    public class TestBlurScreenView : ScreenView
    {
        /// <summary>
        ///     Blur container.
        /// </summary>
        public BlurContainer Blur { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestBlurScreenView(Screen screen) : base(screen)
        {
            // Creates a blur container, any child sprite
            // will be under the blur effect.
            Blur = new BlurContainer(BlurType.Gaussian, 10)
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
            };

            // Create child BackgroundImage to have the blur effect.
            var wallpaper = new BackgroundImage(WobbleAssets.Wallpaper, 10) { Parent = Blur };

            // Create a red box, this should NOT have a blur effect given that it isn't a child
            // of Blur.
            var redBox = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Red,
                Size = new ScalableVector2(100, 100)
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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}