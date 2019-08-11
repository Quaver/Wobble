using System;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Debugging;
using Wobble.Managers;
using Wobble.Screens;
using Alignment = Wobble.Graphics.Alignment;

namespace Wobble.Tests.Screens.Tests.SpriteTextPlusNew
{
    public class TestSpriteTextPlusScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private WobbleFontStore Font { get; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestSpriteTextPlusScreenView(Screen screen) : base(screen)
        {
            Font = FontManager.GetWobbleFont("exo2-semibold");

            var text = new SpriteTextPlus(Font, "Hello, World! いろはにほへど\nthis should be on a new line\n🍆😀 😁 😂 🤣😃 😄 😅 😆 😉 😊 😋 😎\nhi",
                48)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
            };

            text.AddBorder(Color.Crimson, 2);

            text.MoveToY(-300, Easing.Linear, 2000);

            var cyrillic = new SpriteTextPlus(Font, "Лорем ипсум долор сит амет, еяуидем маиорум медиоцрем ут дуо", 22)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = text.Height + 20,
                Tint = Color.Lime
            };

            cyrillic.AddBorder(Color.Cyan, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}