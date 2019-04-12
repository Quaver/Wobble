using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Scaling
{
    public class TestScalingScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestScalingScreenView(Screen screen) : base(screen)
        {
            // 1-wide lines with 1-wide gaps.
            for (var x = 2; x < 100; x += 2)
            {
                new Sprite()
                {
                    Parent = Container,
                    X = x,
                    Y = 1,
                    Width = 1,
                    Height = 50,
                };
            }

            // 0.5-wide lines with 1-wide gaps.
            // Might be invisible on 1366×, but should be visible on 1920× (~0.7, rounds to 1) and above.
            // Should be visible as various line patterns on resolutions below and above.
            for (var x = 2; x < 100; x += 2)
            {
                new Sprite()
                {
                    Parent = Container,
                    X = x,
                    Y = 70,
                    Width = 0.5f,
                    Height = 50,
                };
            }

            // Draws a 50×50 white square.
            new Sprite()
            {
                Parent = Container,
                X = 1,
                Y = 140,
                Width = 50,
                Height = 50,
            };

            // Draws a 46×46 red square within that white square, resulting in a 2-wide outline.
            var redArea = new Sprite()
            {
                Parent = Container,
                X = 3,
                Y = 142,
                Width = 46,
                Height = 46,
                Tint = Color.Red,
            };

            // Draws a centered with MidCenter 45×45 box within that 46×46 area.
            // On 1366×768 it results in 1 px-wide gaps on two sides and no gaps on two other sides.
            // On 2× resolution (2732×1536) where there's enough pixels to add gaps on all four sides, there should be
            // gaps on all four sides, rather than no gaps on two sides and 3 and 2px-wide gaps on the other two sides.
            new Sprite()
            {
                Parent = redArea,
                X = 0,
                Y = 0,
                Width = 45f,
                Height = 45f,
                Alignment = Alignment.MidCenter,
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

            GameBase.Game.SpriteBatch.End();
        }

        public override void Destroy() => Container?.Destroy();
    }
}
