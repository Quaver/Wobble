using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TextVerticalLayout
{
    public class TestTextVerticalLayoutScreenView : ScreenView
    {
        private const float ColumnWidth = 540;
        private readonly WobbleFontStore font = FontManager.GetWobbleFont("inter-semibold");

        public TestTextVerticalLayoutScreenView(Screen screen) : base(screen)
        {
            AddLabel("TEXT VERTICAL LAYOUT", 28, 30, Alignment.TopCenter, Color.White);
            AddLabel("Bounds are blue; capital area is between the green guides. Cached and uncached should match.",
                15, 68, Alignment.TopCenter, new Color(170, 184, 204));

            AddColumnHeader("CACHED", 55);
            AddColumnHeader("UNCACHED", 625);

            CreateComparisonRow("HAMBURG", 22, 120);
            CreateComparisonRow("gjpqy", 22, 205);
            CreateComparisonRow("Ag Hgj pqy", 32, 290);
            CreateComparisonRow("H\n\ngj", 22, 400);
            CreateComparisonRow("Hgj", 48, 535);
        }

        private void CreateComparisonRow(string value, int size, float y)
        {
            CreateSample(value, size, 55, y, true);
            CreateSample(value, size, 625, y, false);
        }

        private void CreateSample(string value, int size, float x, float y, bool cached)
        {
            var text = new SpriteTextPlus(font, value, size, cached)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = x + 12,
                Y = y,
                Tint = Color.White
            };

            AddOutline(x, y, ColumnWidth, text.Height, new Color(55, 135, 220));

            AddGuide(x, y + text.CapTopOffset, Color.LimeGreen);
            AddGuide(x, y + text.CapTopOffset + text.CapHeight, Color.LimeGreen);

            AddLabel($"{size}px  height {text.Height:0.##}  cap {text.CapHeight:0.##}", 13,
                y + text.Height + 4, Alignment.TopLeft, new Color(145, 160, 180), x);
        }

        private void AddOutline(float x, float y, float width, float height, Color color)
        {
            AddBar(x, y, width, 1, color);
            AddBar(x, y + height - 1, width, 1, color);
            AddBar(x, y, 1, height, color);
            AddBar(x + width - 1, y, 1, height, color);
        }

        private void AddGuide(float x, float y, Color color) => AddBar(x, y, ColumnWidth, 1, color);

        private void AddBar(float x, float y, float width, float height, Color color) => new Sprite
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            Image = WobbleAssets.WhiteBox,
            Position = new ScalableVector2(x, y),
            Size = new ScalableVector2(width, height),
            Tint = color
        };

        private void AddColumnHeader(string text, float x) => AddLabel(text, 16, 96,
            Alignment.TopLeft, Color.CornflowerBlue, x);

        private void AddLabel(string text, int size, float y, Alignment alignment, Color tint, float x = 0) =>
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), text, size)
            {
                Parent = Container,
                Alignment = alignment,
                X = x,
                Y = y,
                Tint = tint
            };

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(17, 24, 32));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
