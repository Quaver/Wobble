using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.NineSliceSprite
{
    public class TestNineSliceSpriteScreenView : ScreenView
    {
        public TestNineSliceSpriteScreenView(Screen screen) : base(screen)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "NINE-SLICE SPRITE", 26)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 30,
                Tint = Color.White
            };

            CreateExample(new ScalableVector2(180, 80), -260, Color.CornflowerBlue, "180 x 80");
            CreateExample(new ScalableVector2(420, 80), 40, Color.MediumPurple, "420 x 80");
            CreateExample(new ScalableVector2(180, 260), -260, Color.SeaGreen, "180 x 260", 180);
            CreateExample(new ScalableVector2(420, 260), 40, Color.OrangeRed, "420 x 260", 180);
        }

        private void CreateExample(ScalableVector2 size, float x, Color tint, string label, float y = -80)
        {
            var sprite = new Graphics.Sprites.NineSliceSprite(Textures.RectangleAlphaMask,
                new SliceMargins(70, 70, 60, 60))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Position = new ScalableVector2(x, y),
                Size = size,
                Tint = tint
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), label, 18)
            {
                Parent = sprite,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(17, 24, 32));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
