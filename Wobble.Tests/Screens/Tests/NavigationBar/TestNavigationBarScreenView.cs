using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
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

        private static readonly Color PageBackground = new Color(10, 13, 18);
        private static readonly Color ButtonBackground = new Color(39, 48, 56);

        private NavigationBar TopBar { get; }

        private NavigationBar BottomBar { get; }

        private SpriteTextPlus StatusText { get; }

        private string Status
        {
            get => StatusText.Text;
            set => StatusText.Text = value;
        }

        public TestNavigationBarScreenView(Screen screen) : base(screen)
        {
            TopBar = CreateBar(Alignment.TopLeft);
            BottomBar = CreateBar(Alignment.BotLeft);

            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), "Ready", 20)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            AddIconButton(TopBar, NavigationBarRegion.Left, "Play", true, true);
            AddIconButton(TopBar, NavigationBarRegion.Left, "Tools", true, false,
                CreateDropdownOptions("Editor", "Import", "Export"));
            AddIconButton(TopBar, NavigationBarRegion.Left, "Chat", true);
            AddIconButton(TopBar, NavigationBarRegion.Left, "Favorite", true);

            var profileButton = TopBar.AddRoundedButton(NavigationBarRegion.Right, new NavigationBarButtonOptions
            {
                Icon = Textures.Home,
                IconSize = new Vector2(16, 16),
                Text = "[WWWW] Nickname",
                Font = FontManager.GetWobbleFont("inter-bold"),
                FontSize = 12,
                WidthMode = ButtonSizeMode.Auto,
                Height = 26,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                AntiAliasedEdges = false,
                ClickAction = (sender, args) => Status = "Profile"
            });

            // const float profileRightPadding = 12;
            // profileButton.Width += profileRightPadding;
            // profileButton.Icon.X -= profileRightPadding / 2;
            // profileButton.Label.X -= profileRightPadding / 2;
            TopBar.RefreshLayout();

            AddIconButton(TopBar, NavigationBarRegion.Right, "Menu");

            AddIconButton(BottomBar, NavigationBarRegion.Left, "Website");
            AddIconButton(BottomBar, NavigationBarRegion.Left, "Discord");
            AddIconButton(BottomBar, NavigationBarRegion.Left, "GitHub");

            AddIconButton(BottomBar, NavigationBarRegion.Right, "Volume");
            AddIconButton(BottomBar, NavigationBarRegion.Right, "Settings", false, false,
                CreateDropdownOptions("Graphics", "Audio", "Input"));
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

        private void AddIconButton(NavigationBar bar, NavigationBarRegion region, string action,
            bool expandLabelOnHover = false, bool alwaysShowLabel = false,
            NavigationBarDropdownOption[] dropdownOptions = null)
        {
            bar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                Icon = Textures.Home,
                IconSize = new Vector2(16, 16),
                Text = expandLabelOnHover ? action : null,
                Font = expandLabelOnHover || dropdownOptions != null
                    ? FontManager.GetWobbleFont("inter-bold")
                    : null,
                FontSize = 12,
                Width = 26,
                Height = 26,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                AntiAliasedEdges = false,
                ExpandLabelOnHover = expandLabelOnHover,
                AlwaysShowLabel = alwaysShowLabel,
                HoverExpansionDuration = 150,
                ExpandedLabelRightPadding = 0,
                ClickAction = (sender, args) => Status = action,
                DropdownOptions = dropdownOptions
            });
        }

        private NavigationBarDropdownOption[] CreateDropdownOptions(params string[] actions)
        {
            var options = new NavigationBarDropdownOption[actions.Length];

            for (var i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                options[i] = new NavigationBarDropdownOption
                {
                    Text = action,
                    ClickAction = (sender, args) => Status = action
                };
            }

            return options;
        }

        private void RefreshViewportLayout()
        {
            TopBar.Width = WindowManager.Width;
            BottomBar.Width = WindowManager.Width;
        }
    }
}
