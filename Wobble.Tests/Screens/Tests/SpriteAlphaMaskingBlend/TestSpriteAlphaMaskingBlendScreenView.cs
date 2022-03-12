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
            // Create the Sprite that will get masked
            var maskedSprite = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(500, 200),
                Image = WobbleAssets.Wallpaper
            };

            var maskedText = new SpriteText("exo2-bold", "This is masked!", 16)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 100
            };

            // Create the Mask texture to blend with the Sprite
            var CircleMask = Textures.CircleAlphaMask;
            var RectangleMask = Textures.RectangleAlphaMask;

            // Perform the blend between the Sprite and the Mask
            maskedSprite.Image = SpriteAlphaMaskBlend.PerformBlend(maskedSprite.Image, CircleMask);
            maskedText.Image = SpriteAlphaMaskBlend.PerformBlend(maskedText.Image, RectangleMask);
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
