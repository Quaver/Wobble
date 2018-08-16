using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.Blur
{
    public class TestBlurScreenView : ScreenView
    {
        public TestBlurScreenView(Screen screen) : base(screen)
        {
            var container = new RenderTargetContainer()
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Alpha = 1
            };

            var child = new Sprite()
            {
                Parent = container,
                Size = new ScalableVector2(300, 300),
                Alignment = Alignment.TopCenter,
                Y = 150,
                Tint = Color.Green
            };

            new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(50, 50),
                Tint = Color.Red,
                Alignment = Alignment.MidRight
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