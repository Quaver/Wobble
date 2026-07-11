using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Selection
{
    public class SelectionScreenView : ScreenView
    {
        private readonly SpriteTextPlus _title;
        private readonly SpriteTextPlus _help;

        public SelectionScreenView(Screen screen) : base(screen)
        {
            _title = new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"),
                LocalizationManager.Get("Selection_Title"), 34)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -22,
                Tint = Color.White
            };

            _help = new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                LocalizationManager.Get("Selection_WelcomeHelp"), 18)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 28,
                Tint = new Color(174, 184, 196)
            };
        }

        public void RefreshLocalizedText()
        {
            _title.Text = LocalizationManager.Get("Selection_Title");
            _help.Text = LocalizationManager.Get("Selection_WelcomeHelp");
        }

        public override void Update(GameTime gameTime) => Container.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(14, 18, 24));
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();
    }
}
