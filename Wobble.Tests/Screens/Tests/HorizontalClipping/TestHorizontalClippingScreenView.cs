using System;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.HorizontalClipping
{
    public class TestHorizontalClippingScreenView : ScreenView
    {
        private FilledRectangleSprite MovingSprite { get; }

        public TestHorizontalClippingScreenView(Screen screen) : base(screen)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "HORIZONTAL CLIPPING CONTAINER", 26)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 30,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "Animated overflow, nested clipping, and render-state restoration", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 68,
                Tint = Color.LightGray
            };

            var outer = new HorizontalClippingContainer
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                X = -260,
                Size = new ScalableVector2(360, 240)
            };

            new FilledRectangleSprite
            {
                Parent = outer,
                Position = new ScalableVector2(-50, -35),
                Size = new ScalableVector2(460, 310),
                Tint = new Color(38, 67, 92),
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"),
                "THIS TEXT EXTENDS BEYOND THE OUTER CLIP ON BOTH SIDES", 22)
            {
                Parent = outer,
                X = -90,
                Y = 25,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            MovingSprite = new FilledRectangleSprite
            {
                Parent = outer,
                Position = new ScalableVector2(-100, 90),
                Size = new ScalableVector2(130, 55),
                Tint = Color.Orange,
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            var nested = new HorizontalClippingContainer
            {
                Parent = outer,
                Position = new ScalableVector2(205, 145),
                Size = new ScalableVector2(230, 70)
            };

            new FilledRectangleSprite
            {
                Parent = nested,
                Position = new ScalableVector2(-35, -20),
                Size = new ScalableVector2(320, 120),
                Tint = Color.MediumPurple,
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "NESTED CLIP", 20)
            {
                Parent = nested,
                X = 15,
                Y = 20,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            // This sibling is deliberately drawn after the clipping container. If it is clipped,
            // the clipping container leaked its render state.
            new FilledRectangleSprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Position = new ScalableVector2(170, -80),
                Size = new ScalableVector2(310, 160),
                Tint = new Color(31, 124, 78),
                Pivot = Vector2.Zero
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "UNCLIPPED SIBLING", 22)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                X = 325,
                Tint = Color.White
            };
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            MovingSprite.X = -100 + (float)((Math.Sin(gameTime.TotalGameTime.TotalSeconds * 1.5) + 1) * 210);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(17, 24, 32));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
