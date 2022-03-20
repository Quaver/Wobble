using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.SpriteAlphaMaskingBlend
{
    public class TestSpriteAlphaMaskingBlendScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestSpriteAlphaMaskingBlendScreenView(Screen screen) : base(screen)
        {
            // Create the masked sprite
            var maskedSprite = new SpriteAlphaMaskBlend()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(500, 200)
            };

            var textSprite = new SpriteText("exo2-bold", "This is masked!", 16);

            var maskedText = new SpriteAlphaMaskBlend()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = textSprite.Size,
                Y = 100
            };

            // Perform the blend between the Sprite and the Mask
            maskedSprite.Image = maskedSprite.PerformBlend(WobbleAssets.Wallpaper, Textures.CircleAlphaMask);
            maskedText.Image = maskedText.PerformBlend(textSprite.Image, Textures.RectangleAlphaMask);
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
