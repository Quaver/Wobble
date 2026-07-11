using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Tests.Screens.Selection
{
    internal static class TestNavigation
    {
        private const string NavbarKey = "wobble-tests-navbar";
        private const float NavbarHeight = 48;

        private static readonly Color NavbarBackground = new Color(20, 25, 32);
        private static readonly Color ButtonBackground = new Color(42, 51, 62);
        private static readonly Color DropdownBackground = new Color(25, 31, 40);

        public static void AttachTo(Screen screen)
        {
            ApplyContentInset(screen.View.Container);

            if (!ScreenManager.TryGetElement<TestNavigationBar>(NavbarKey, out var navbar))
            {
                navbar = new TestNavigationBar(WindowManager.Width, NavbarHeight)
                {
                    Parent = screen.View.Container,
                    Alignment = Alignment.TopLeft,
                    Y = -NavbarHeight,
                    EdgePadding = 12,
                    ItemSpacing = 7,
                    BackgroundColor = NavbarBackground
                };

                ScreenManager.RegisterElement(NavbarKey, navbar);
            }

            navbar.Y = -NavbarHeight;
            Populate(navbar);
        }

        private static void Populate(TestNavigationBar navbar)
        {
            navbar.Clear(destroy: true);

            AddButton(navbar, NavigationBarRegion.Left, LocalizationManager.Get("Selection_Title"),
                () => Navigate(new SelectionScreen(), false));

            foreach (var category in TestScreenRegistry.Categories)
            {
                var entries = TestScreenRegistry.Screens.Where(x => x.CategoryKey == category)
                    .Select(x => new NavigationBarDropdownOption
                    {
                        Text = LocalizationManager.Get(x.LabelKey),
                        ClickAction = (sender, args) => Navigate(x.CreateScreen(), x.Isolated)
                    }).ToArray();

                AddButton(navbar, NavigationBarRegion.Left, LocalizationManager.Get(category), null, entries);
            }

            AddButton(navbar, NavigationBarRegion.Right, LocalizationManager.Get("Selection_LanguageLabel"), null,
                new[]
                {
                    LanguageOption("Language_English", "en"),
                    LanguageOption("Language_Bulgarian", "bg")
                });
        }

        private static NavigationBarDropdownOption LanguageOption(string labelKey, string cultureName) =>
            new NavigationBarDropdownOption
            {
                Text = LocalizationManager.Get(labelKey),
                ClickAction = (sender, args) =>
                {
                    LocalizationManager.SetCurrentCulture(CultureInfo.GetCultureInfo(cultureName));

                    if (ScreenManager.TryGetElement<TestNavigationBar>(NavbarKey, out var navbar))
                        Populate(navbar);

                    if (ScreenManager.CurrentScreenName == nameof(SelectionScreen) &&
                        ScreenManager.TryGetElement<TestNavigationBar>(NavbarKey, out _))
                    {
                        // The welcome view is the only regular screen with localized body copy.
                        // Re-enter it while retaining the navbar so both surfaces refresh together.
                        Navigate(new SelectionScreen(), false);
                    }
                }
            };

        private static void Navigate(Screen screen, bool isolated)
        {
            if (isolated)
            {
                ScreenManager.ChangeScreen(screen);
                return;
            }

            ApplyContentInset(screen.View.Container);
            ScreenManager.ChangeScreen(screen, new[] { NavbarKey });
        }

        private static void ApplyContentInset(Container container)
        {
            container.Position = new ScalableVector2(0, NavbarHeight);
            container.Size = new ScalableVector2(WindowManager.Width,
                System.Math.Max(1, WindowManager.Height - NavbarHeight));
        }

        private static void AddButton(NavigationBar navbar, NavigationBarRegion region, string text,
            System.Action action, IReadOnlyList<NavigationBarDropdownOption> dropdown = null)
        {
            navbar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                Text = text,
                Font = FontManager.GetWobbleFont("inter-semibold"),
                FontSize = 12,
                WidthMode = ButtonSizeMode.Auto,
                Height = 30,
                AutoSizePadding = new Vector2(14, 0),
                CornerRadius = 3,
                ForegroundColor = Color.White,
                BackgroundColor = action == null && dropdown == null ? Color.Transparent : ButtonBackground,
                DropdownBackgroundColor = DropdownBackground,
                DropdownItemBackgroundColor = ButtonBackground,
                DropdownForegroundColor = Color.White,
                DropdownOptions = dropdown,
                AntiAliasedEdges = false,
                ClickAction = action == null ? null : (sender, args) => action()
            });
        }

        private sealed class TestNavigationBar : NavigationBar
        {
            public TestNavigationBar(float width, float height) : base(width, height)
            {
            }

            public override void Update(GameTime gameTime)
            {
                if (Parent is Container container)
                {
                    container.Position = new ScalableVector2(0, NavbarHeight);
                    container.Size = new ScalableVector2(WindowManager.Width,
                        System.Math.Max(1, WindowManager.Height - NavbarHeight));
                    Width = WindowManager.Width;
                    Y = -NavbarHeight;
                }

                base.Update(gameTime);
            }
        }
    }
}
