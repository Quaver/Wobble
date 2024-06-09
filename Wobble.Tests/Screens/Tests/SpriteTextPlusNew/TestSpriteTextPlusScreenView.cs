using System;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        private SpriteTextPlus wrapped;

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
                Alignment = Alignment.MidLeft,
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

            var ltr = new SpriteTextPlus(Font, "This text is aligned from\nleft to right", 22)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                TextAlignment = TextAlignment.Left,
                X = 20,
                Y = 100
            };

            ltr.AddBorder(Color.White, 2);

            // ReSharper disable once ObjectCreationAsStatement
            var rtl = new SpriteTextPlus(Font, "This text is aligned from\nright to left", 22)
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
                TextAlignment = TextAlignment.Right,
                X = -20,
                Y = 100
            };

            rtl.AddBorder(Color.White, 2);

            // ReSharper disable once ObjectCreationAsStatement
            var center = new SpriteTextPlus(Font, "This text is aligned\nin the center", 22)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center,
                Y = 100
            };

            center.AddBorder(Color.White, 2);

            wrapped = new SpriteTextPlus(Font,
                "This text is too long and will be wrapped. Also AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA.\n\n     Hello      there         as well          !         ",
                22)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                TextAlignment = TextAlignment.Left,
                MaxWidth = 100
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalSeconds % 6 > 3)
                wrapped.MaxWidth = 1 + 899 * (float) (gameTime.TotalGameTime.TotalSeconds % 3) / 3;
            else
                wrapped.MaxWidth = 1 + 899 * (1 - (float) (gameTime.TotalGameTime.TotalSeconds % 3) / 3);

            wrapped.X = Container.Width - wrapped.MaxWidth.Value;

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