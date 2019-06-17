using System;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Debugging;
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
            Font = new WobbleFontStore(20, GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-semibold.ttf"));
            Font.AddFont("Emoji", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/symbola-emoji.ttf"));
            Font.AddFont("Japanese", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/droid-sans-japanese.ttf"));


            var text = new SpriteTextPlus(Font, "Hello, World! いろはにほへど \n this should be on a new line 🍆😀 😁 😂 🤣😃 😄 😅 😆 😉 😊 😋 😎",
                32)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            text.AddBorder(Color.Crimson, 2);

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