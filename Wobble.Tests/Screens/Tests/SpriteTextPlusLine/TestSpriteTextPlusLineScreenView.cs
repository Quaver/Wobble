using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics;
using Wobble.Managers;
using Wobble.Screens;
using Color = Microsoft.Xna.Framework.Color;

namespace Wobble.Tests.Screens.Tests.TextLine
{
    public class TestSpriteTextPlusLineScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestSpriteTextPlusLineScreenView(Screen screen) : base(screen)
        {
            new SpriteTextPlusLine(FontManager.GetWobbleFont("exo2-regular"),
                "The quick brown fox jumps over the lazy dog", 72)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };

            new SpriteTextPlusLine(FontManager.GetWobbleFont("exo2-regular"),
                "The quick brown fox jumps over the lazy dog", 12)
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
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
        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}
