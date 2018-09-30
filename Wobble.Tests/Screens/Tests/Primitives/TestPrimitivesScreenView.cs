using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Primitives
{
    public class TestPrimitivesScreenView : ScreenView
    {
        /// <summary>
        ///     Simple horizontal line displayed at the top of the screen.
        /// </summary>
        public Line HorizontalLine { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestPrimitivesScreenView(Screen screen) : base(screen)
        {
            HorizontalLine = new Line(Vector2.One, Color.White, 1)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Alpha = 0.5f,
                X = -250,
                Y = 30
            };

            HorizontalLine.EndPosition = new Vector2(HorizontalLine.AbsolutePosition.X + 500, HorizontalLine.AbsolutePosition.Y);
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
