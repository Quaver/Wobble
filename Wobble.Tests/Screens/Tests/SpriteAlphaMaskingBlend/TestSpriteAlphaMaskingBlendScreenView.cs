using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            var textSprite = new SpriteText("inter-bold", "This is masked!", 16)
            {
                IsCached = false
            };

            var maskedText = new SpriteAlphaMaskBlend()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = textSprite.Size,
                Y = 100
            };

            // Perform the blend between the Sprite and the Mask
            maskedSprite.Image = maskedSprite.PerformBlend(WobbleAssets.Wallpaper, Textures.CircleAlphaMask);
            var textTexture = RenderTextToTexture(textSprite);
            maskedText.Image = maskedText.PerformBlend(textTexture, Textures.RectangleAlphaMask);
            textTexture.Dispose();
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

        private static Texture2D RenderTextToTexture(SpriteText text)
        {
            var width = Math.Max(1, (int)Math.Ceiling(text.Width));
            var height = Math.Max(1, (int)Math.Ceiling(text.Height));
            var renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, width, height, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            text.Alignment = Alignment.TopLeft;
            text.Position = new ScalableVector2(0, 0);

            var oldRenderTargets = GameBase.Game.GraphicsDevice.GetRenderTargets();
            _ = GameBase.Game.TryEndBatch();

            GameBase.Game.GraphicsDevice.SetRenderTarget(renderTarget);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

            GameBase.Game.SpriteBatch.Begin();
            text.DrawToSpriteBatch();
            GameBase.Game.SpriteBatch.End();

            GameBase.Game.GraphicsDevice.SetRenderTargets(oldRenderTargets);

            return renderTarget;
        }
    }
}
