using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Screens.Selection;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.PersistentElements
{
    public enum TestPersistentElementsStage
    {
        First,
        Second
    }

    public sealed class TestPersistentElementsScreenView : ScreenView
    {
        private const string NavbarKey = "persistent-elements-navbar";
        private const float NavbarHeight = 48;

        private static readonly Color FirstBackground = new Color(32, 54, 74);
        private static readonly Color SecondBackground = new Color(52, 30, 64);
        private static readonly Color OpaqueNavbarBackground = new Color(18, 21, 27);
        private static readonly Color ButtonBackground = new Color(58, 69, 82);

        private SpriteTextPlus StatusText { get; }

        private TestPersistentElementsStage Stage { get; set; }

        public TestPersistentElementsScreenView(Screen screen) : base(screen)
        {
            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), string.Empty, 20)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
        }

        public void Configure(TestPersistentElementsStage stage)
        {
            Stage = stage;

            var reused = ScreenManager.TryGetElement<NavigationBar>(NavbarKey, out var navbar);

            if (!reused)
            {
                navbar = new NavigationBar(WindowManager.Width, NavbarHeight)
                {
                    Parent = Container,
                    Alignment = Alignment.TopLeft,
                    EdgePadding = 12,
                    ItemSpacing = 8
                };

                ScreenManager.RegisterElement(NavbarKey, navbar);
            }

            navbar.Clear(destroy: true);
            navbar.BackgroundColor = stage == TestPersistentElementsStage.First
                ? Color.Transparent
                : OpaqueNavbarBackground;

            if (stage == TestPersistentElementsStage.First)
            {
                AddButton(navbar, NavigationBarRegion.Left, "First screen", null);
                AddButton(navbar, NavigationBarRegion.Right, "Next (keep)",
                    () => ScreenManager.ChangeScreen(new TestPersistentElementsSecondScreen(),
                        new[] { NavbarKey }));
            }
            else
            {
                AddButton(navbar, NavigationBarRegion.Left, "Second screen", null);
                AddButton(navbar, NavigationBarRegion.Right, "Back (keep)",
                    () => ScreenManager.ChangeScreen(new TestPersistentElementsFirstScreen(),
                        new[] { NavbarKey }));
            }

            AddButton(navbar, NavigationBarRegion.Right, LocalizationManager.Get("Navigation_BackToTests"),
                () => ScreenManager.ChangeScreen(new SelectionScreen()));

            StatusText.Text = $"{(stage == TestPersistentElementsStage.First ? "FIRST" : "SECOND")} SCREEN\n" +
                              $"Navbar instance: {RuntimeHelpers.GetHashCode(navbar)}\n" +
                              $"Navbar reused: {(reused ? "yes" : "no")}\n" +
                              $"Background: {(stage == TestPersistentElementsStage.First ? "transparent" : "opaque")}\n\n" +
                              "Use the navbar buttons to retain it or remove it.";
        }

        public override void Update(GameTime gameTime)
        {
            if (ScreenManager.TryGetElement<NavigationBar>(NavbarKey, out var navbar))
                navbar.Width = WindowManager.Width;

            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Stage == TestPersistentElementsStage.First
                ? FirstBackground
                : SecondBackground);
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();

        private static RoundedButton AddButton(NavigationBar navbar, NavigationBarRegion region,
            string text, Action action)
        {
            return navbar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                Text = text,
                Font = FontManager.GetWobbleFont("inter-semibold"),
                FontSize = 12,
                WidthMode = ButtonSizeMode.Auto,
                Height = 30,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = action == null ? Color.Transparent : ButtonBackground,
                AntiAliasedEdges = false,
                ClickAction = action == null ? null : (sender, args) => action()
            });
        }
    }
}
