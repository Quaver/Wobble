using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using MarqueeText = Wobble.Graphics.Sprites.Text.MarqueeSpriteText;

namespace Wobble.Tests.Screens.Tests.MarqueeSpriteText
{
    public class TestMarqueeSpriteTextScreenView : ScreenView
    {
        private MarqueeText ToggleMarquee { get; }

        private MarqueeText HoverMarquee { get; }

        private SpriteTextPlus ToggleState { get; }

        public TestMarqueeSpriteTextScreenView(Screen screen) : base(screen)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "MARQUEE SPRITE TEXT", 26)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 30,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"), "Press SPACE to toggle the first marquee", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 68,
                Tint = Color.LightGray
            };

            ToggleMarquee = CreateRow("ACTIVE / TOGGLE", 130,
                "This long marquee scrolls left, waits, and returns to its starting position", 20, 390, true);
            ToggleState = CreateStateText("Active: YES", 130);

            CreateRow("SHORT TEXT", 225, "Fits without scrolling", 20, 390, true);
            CreateRow("EXACT WIDTH", 320, "Exactly measured width", 20, 390, true, true);
            CreateRow("INACTIVE OVERFLOW", 415,
                "This overflowing text remains fixed because its marquee is inactive", 20, 390, false);
            CreateRow("NARROW / LARGE FONT", 510,
                "Large text moving through a narrow viewport", 28, 260, true);
            HoverMarquee = CreateRow("HOVER ACTIVATED", 605,
                "This marquee only scrolls while the mouse pointer is hovering over it", 20, 390, false);
            HoverMarquee.StartDelayMilliseconds = 0;

            new FilledRectangleSprite
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
                Y = -15,
                Size = new ScalableVector2(660, 45),
                Tint = new Color(31, 124, 78),
                Pivot = Vector2.Zero
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"),
                "UNCLIPPED SIBLING AFTER ALL MARQUEES", 18)
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
                Y = -25,
                Tint = Color.White
            };
        }

        private MarqueeText CreateRow(string label, float y, string text, int fontSize, float width, bool active,
            bool matchTextWidth = false)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), label, 15)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = -330,
                Y = y,
                Tint = Color.LightGray
            };

            var background = new FilledRectangleSprite
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = 80,
                Y = y - 8,
                Size = new ScalableVector2(width + 24, fontSize + 34),
                Tint = new Color(38, 52, 67),
                Pivot = Vector2.Zero
            };

            var marquee = new MarqueeText(FontManager.GetWobbleFont("inter-medium"), text, fontSize, width)
            {
                Parent = background,
                Alignment = Alignment.MidCenter,
                IsActive = active
            };

            if (matchTextWidth)
            {
                marquee.Width = marquee.TextSprite.Width;
                background.Width = marquee.Width + 24;
            }

            return marquee;
        }

        private SpriteTextPlus CreateStateText(string text, float y) =>
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), text, 15)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = 360,
                Y = y,
                Tint = Color.LightGreen
            };

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Space))
            {
                ToggleMarquee.IsActive = !ToggleMarquee.IsActive;
                ToggleState.Text = ToggleMarquee.IsActive ? "Active: YES" : "Active: NO";
                ToggleState.Tint = ToggleMarquee.IsActive ? Color.LightGreen : Color.IndianRed;
            }

            HoverMarquee.IsActive = HoverMarquee.Parent.IsHovered();
            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(17, 24, 32));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
