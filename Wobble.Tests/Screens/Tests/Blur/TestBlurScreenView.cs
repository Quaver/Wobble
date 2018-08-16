using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.Blur
{
    public class TestBlurScreenView : ScreenView
    {
        /// <summary>
        ///     Blur container.
        /// </summary>
        public BlurContainer Blur { get; }

        /// <summary>
        ///     Text that displays the current blur strength.
        /// </summary>
        public SpriteText BlurStrengthText { get; }

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
                Children =
                {
                    // Create a child wallpaper to be placed inside of the blur container.
                    new BackgroundImage(WobbleAssets.Wallpaper, 10) { Parent = Blur }
                }
            };

            // Create a red box, this should NOT have a blur effect given that it isn't a child
            // of Blur.
            var redBox = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Red,
                Size = new ScalableVector2(100, 100)
            };

            BlurStrengthText = new SpriteText(Fonts.AllerRegular16, $"Blur Strength: {Blur.Strength}")
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                TextColor = Color.LimeGreen,
                Y = 15,
                TextScale = 1.25f
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
             // 1
            if (KeyboardManager.IsUniqueKeyPress(Keys.D1))
                Blur.BlurType = BlurType.Gaussian;

            // 2
            if (KeyboardManager.IsUniqueKeyPress(Keys.D2))
                Blur.BlurType = BlurType.Frosty;

            // 3
            if (KeyboardManager.IsUniqueKeyPress(Keys.D3))
                Blur.BlurType = BlurType.Fast;

            // Turn blur strength down
            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                Blur.Strength -= 1;

            // Turn blur strength up.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                Blur.Strength += 1;

            BlurStrengthText.Text = $"Blur Strength: {Blur.Strength}";
            Container?.Update(gameTime);
        }

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