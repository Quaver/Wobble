using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.SpriteAlphaMaskingBlend
{
    public class TestSpriteAlphaMaskingBlendScreenView : ScreenView
    {
        private readonly SpriteAlphaMaskBlend maskedText;
        private readonly SpriteTextPlus textSprite;
        private bool textBlendCreated;

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

            textSprite = new SpriteTextPlus("exo2-bold", "This is masked!", 16)
            {
                Parent = Container,
                Visible = false
            };

            maskedText = new SpriteAlphaMaskBlend()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = textSprite.Size,
                Y = 100
            };

            // Perform the blend between the Sprite and the Mask
            maskedSprite.Image = maskedSprite.PerformBlend(WobbleAssets.Wallpaper, Textures.CircleAlphaMask);
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
            CreateTextBlendWhenReady();

            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        private void CreateTextBlendWhenReady()
        {
            if (textBlendCreated || textSprite.Children.Count == 0)
                return;

            var line = textSprite.Children[0] as SpriteTextPlusLine;
            if (!(line?.Image is RenderTarget2D))
                return;

            maskedText.Image = maskedText.PerformBlend(line.Image, Textures.RectangleAlphaMask);
            textBlendCreated = true;
            textSprite.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}
