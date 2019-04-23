using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TextSizes
{
    public class TestTextSizesScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestTextSizesScreenView(Screen screen) : base(screen)
        {
            new Sprite
            {
                Parent = Container,
                Tint = Color.SkyBlue,
                Width = Container.Width,
                Height = Container.Height,
            };

            float y = 0;
            for (int i = 1; i < 30; i++)
            {
                var line = new SpriteText("exo2-regular", "The quick brown fox jumps over the lazy dog", i)
                {
                    Parent = Container,
                    Y = y,
                };
                y += line.Height;
            }
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

            GameBase.Game.SpriteBatch.End();
        }

        public override void Destroy() => Container?.Destroy();
    }
}
