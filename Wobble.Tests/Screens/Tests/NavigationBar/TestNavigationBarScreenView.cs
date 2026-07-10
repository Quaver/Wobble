using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.NavigationBars
{
    public class TestNavigationBarScreenView : ScreenView
    {
        private const float BarHeight = 40;

        private static readonly Color PageBackground = new Color(250, 250, 250);
        private static readonly Color ButtonBackground = new Color(80, 80, 80);

        private NavigationBar TopBar { get; }

        private NavigationBar BottomBar { get; }

        private string Status { get; set; } = "Ready";

        public TestNavigationBarScreenView(Screen screen) : base(screen)
        {
            TopBar = CreateBar(Alignment.TopLeft);
            BottomBar = CreateBar(Alignment.BotLeft);

            AddIconButton(TopBar, NavigationBarRegion.Left, "Play");
            AddIconButton(TopBar, NavigationBarRegion.Left, "Tools");
            AddIconButton(TopBar, NavigationBarRegion.Left, "Chat");
            AddIconButton(TopBar, NavigationBarRegion.Left, "Favorite");

            var profileButton = TopBar.AddRoundedButton(NavigationBarRegion.Right, new NavigationBarButtonOptions
            {
                Icon = Textures.Home,
                IconSize = new Vector2(16, 16),
                Text = "[WWWW] Nickname",
                Font = FontManager.GetWobbleFont("inter-bold"),
                FontSize = 11,
                WidthMode = ButtonSizeMode.Auto,
                Height = 26,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                ClickAction = (sender, args) => Status = "Profile"
            });

            const float profileRightPadding = 12;
            profileButton.Width += profileRightPadding;
            profileButton.Icon.X -= profileRightPadding / 2;
            profileButton.Label.X -= profileRightPadding / 2;
            TopBar.RefreshLayout();

            AddIconButton(TopBar, NavigationBarRegion.Right, "Menu");

            AddIconButton(BottomBar, NavigationBarRegion.Left, "Website");
            AddIconButton(BottomBar, NavigationBarRegion.Left, "Discord");
            AddIconButton(BottomBar, NavigationBarRegion.Left, "GitHub");

            AddIconButton(BottomBar, NavigationBarRegion.Right, "Volume");
            AddIconButton(BottomBar, NavigationBarRegion.Right, "Settings");
            AddIconButton(BottomBar, NavigationBarRegion.Right, "Power");
        }

        public override void Update(GameTime gameTime)
        {
            RefreshViewportLayout();
            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(PageBackground);
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();

        private NavigationBar CreateBar(Alignment alignment) => new NavigationBar(
            WindowManager.Width, BarHeight)
        {
            Parent = Container,
            Alignment = alignment,
            EdgePadding = 10,
            ItemSpacing = 7
        };

        private void AddIconButton(NavigationBar bar, NavigationBarRegion region, string action)
        {
            bar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                Icon = Textures.Home,
                IconSize = new Vector2(16, 16),
                Width = 26,
                Height = 26,
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                ClickAction = (sender, args) => Status = action
            });
        }

        private void RefreshViewportLayout()
        {
            TopBar.Width = WindowManager.Width;
            BottomBar.Width = WindowManager.Width;
        }
    }
}
