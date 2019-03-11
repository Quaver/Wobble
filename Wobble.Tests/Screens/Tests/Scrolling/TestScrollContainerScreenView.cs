using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Screens;
using Wobble.Tests.Assets;
using IDrawable = Microsoft.Xna.Framework.IDrawable;

namespace Wobble.Tests.Screens.Tests.Scrolling
{
    public class TestScrollContainerScreenView : ScreenView
    {
        private ScrollContainer ScrollContainer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestScrollContainerScreenView(Screen screen) : base(screen)
        {
            ScrollContainer = new ScrollContainer(new ScalableVector2(400, 400), new ScalableVector2(400, 1000))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                InputEnabled = true
            };

            ScrollContainer.AddContainedDrawable(new Sprite()
            {
                Alignment = Alignment.TopCenter,
                Y = 0,
                Size = new ScalableVector2(100, 100),
                Tint = Color.Black
            });

            ScrollContainer.AddContainedDrawable(new SpriteText("exo2-regular", "I love eggplants.", 12)
            {
                Alignment = Alignment.MidCenter,
                Tint = Color.Purple
            });

            var testText = new SpriteText("exo2-regular", "This should be clipped off-screen!", 12)
            {
                Alignment = Alignment.BotCenter,
                Y = 0,
                Tint = Color.Black
            };

            testText.Y = -testText.Height / 2f;
            ScrollContainer.AddContainedDrawable(testText);
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