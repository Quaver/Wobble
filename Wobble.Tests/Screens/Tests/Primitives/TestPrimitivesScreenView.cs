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

        /// <summary>
        ///     Simple rectangle.
        /// </summary>
        public RectangleSprite Rect { get; }

        /// <summary>
        ///     Simple filled in rectangle.
        /// </summary>
        public FilledRectangleSprite FilledRect { get; }

        /// <summary>
        ///     A list of points drawn in a single line batch.
        ///
        ///     Draws a triangle.
        /// </summary>
        public PrimitiveLineBatch PrimitiveLineBatch { get; }

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

            Rect = new RectangleSprite(5)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 65,
                Size = new ScalableVector2(50, 20)
            };

            FilledRect = new FilledRectangleSprite()
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(0, Rect.Y + Rect.Height + 10),
                Size = new ScalableVector2(100, 30)
            };
            Rect.AddBorder(Color.Green, 2);


            PrimitiveLineBatch = new PrimitiveLineBatch(new List<Vector2>()
            {
                new Vector2(0, 100),
                new Vector2(100, 100),
                new Vector2(100, 0),
                new Vector2(0, 100)
            }, 3)
            {
                Parent = Container,
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
