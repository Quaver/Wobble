using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Debugging;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.ButtonPerformance
{
    public class TestButtonPerformanceScreenView : ScreenView
    {
        private const int VisibleButtonCount = 10;
        private const int ButtonCount = 20;
        private const int ButtonWidth = 220;
        private const int ButtonHeight = 42;
        private const int ButtonSpacing = 10;
        private const int StatsRefreshTime = 100;
        private const int ContentHeight = 770;

        private static readonly Color BackgroundColor = new Color(17, 24, 32);
        private static readonly Color SecondaryTextColor = new Color(184, 195, 204);
        private static readonly Color ControlColor = new Color(31, 41, 51);
        private static readonly Color ScrollbarColor = new Color(86, 97, 107);
        private static readonly Color BlueButtonColor = new Color(15, 186, 229);
        private static readonly Color PurpleButtonColor = new Color(117, 92, 222);
        private static readonly Color StatTextColor = new Color(201, 210, 218);
        private static readonly Color EnabledColor = new Color(105, 230, 166);
        private static readonly Color DisabledColor = new Color(255, 119, 119);

        private Container NewButtonGroup { get; }

        private Container OldButtonGroup { get; }

        private ScrollContainer ScreenScrollContainer { get; }

        private Container ContentContainer => ScreenScrollContainer.ContentContainer;

        private SpriteTextPlus NewStateText { get; }

        private SpriteTextPlus OldStateText { get; }

        private List<SpriteTextPlus> StatLines { get; } = new List<SpriteTextPlus>();

        private List<ScrollContainer> ButtonScrollContainers { get; } = new List<ScrollContainer>();

        private double StatsRefreshTimer { get; set; }

        public TestButtonPerformanceScreenView(Screen screen) : base(screen)
        {
            ScreenScrollContainer = new ScrollContainer(
                new ScalableVector2(Container.Width, Container.Height),
                new ScalableVector2(Container.Width, Math.Max(ContentHeight, Container.Height)))
            {
                Parent = Container,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                Tint = Color.Transparent
            };

            ScreenScrollContainer.Scrollbar.Tint = ScrollbarColor;
            ScreenScrollContainer.Scrollbar.Width = 6;

            CreateHeader();
            CreateToggleControls();

            NewButtonGroup = CreateButtonGroup("NEW ROUNDED BUTTONS", -170, CreateRoundedButton);
            OldButtonGroup = CreateButtonGroup("OLD IMAGE BUTTONS", 170, CreateImageButton);

            NewStateText = CreateGroupStateText("New buttons: ON", -170);
            OldStateText = CreateGroupStateText("Old image buttons: ON", 170);

            CreateStatsPanel();
            UpdateStateText();
            UpdateStatsText();
        }

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "BUTTON PERFORMANCE", 26)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 30,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"), "1: new buttons  |  2: old image buttons  |  R: reset both", 18)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 66,
                Tint = SecondaryTextColor
            };
        }

        private void CreateToggleControls()
        {
            CreateToggleControl("TOGGLE NEW", -240, (sender, args) => ToggleNewButtons());
            CreateToggleControl("TOGGLE OLD", 0, (sender, args) => ToggleOldButtons());
            CreateToggleControl("RESET", 240, (sender, args) => ResetButtons());
        }

        private void CreateToggleControl(string text, float x, EventHandler clicked)
        {
            var button = new ImageButton(WobbleAssets.WhiteBox, clicked)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = 105,
                Size = new ScalableVector2(190, 36),
                Tint = ControlColor
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), text, 15)
            {
                Parent = button,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };
        }

        private Container CreateButtonGroup(string title, float x, Func<int, Drawable> createButton)
        {
            var group = new Container
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = 170,
                Size = new ScalableVector2(ButtonWidth, 40 + VisibleButtonCount * (ButtonHeight + ButtonSpacing) - ButtonSpacing),
                SetChildrenVisibility = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), title, 17)
            {
                Parent = group,
                Alignment = Alignment.TopCenter,
                Tint = Color.White
            };

            var scrollContainer = new ScrollContainer(
                new ScalableVector2(ButtonWidth + 18, VisibleButtonCount * (ButtonHeight + ButtonSpacing) - ButtonSpacing),
                new ScalableVector2(ButtonWidth + 18, ButtonCount * (ButtonHeight + ButtonSpacing) - ButtonSpacing))
            {
                Parent = group,
                Alignment = Alignment.TopCenter,
                Y = 40,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                ScrollSpeed = ButtonHeight + ButtonSpacing,
                Tint = Color.Transparent
            };

            scrollContainer.Scrollbar.Tint = ScrollbarColor;
            scrollContainer.Scrollbar.Width = 6;
            ButtonScrollContainers.Add(scrollContainer);

            for (var i = 0; i < ButtonCount; i++)
            {
                var button = createButton(i);
                button.Alignment = Alignment.TopCenter;
                button.Y = i * (ButtonHeight + ButtonSpacing);
                scrollContainer.AddContainedDrawable(button);
            }

            UpdateScrollButtonVisibility(scrollContainer);
            return group;
        }

        private Drawable CreateRoundedButton(int index)
        {
            var button = new RoundedButton
            {
                Size = new ScalableVector2(ButtonWidth, ButtonHeight),
                Tint = index % 2 == 0 ? BlueButtonColor : PurpleButtonColor,
                PerformHoverFade = false
            };

            button.SetLabel(FontManager.GetWobbleFont("inter-bold"), $"NEW BUTTON {index + 1}", 16, Color.White);
            button.Clicked += (sender, args) => { };

            return button;
        }

        private Drawable CreateImageButton(int index)
        {
            var button = new ImageButton(WobbleAssets.WhiteBox)
            {
                Size = new ScalableVector2(ButtonWidth, ButtonHeight),
                Tint = index % 2 == 0 ? BlueButtonColor : PurpleButtonColor
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), $"OLD BUTTON {index + 1}", 16)
            {
                Parent = button,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };

            button.Clicked += (sender, args) => { };

            return button;
        }

        private SpriteTextPlus CreateGroupStateText(string text, float x) => new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), text, 18)
        {
            Parent = ContentContainer,
            Alignment = Alignment.TopCenter,
            X = x,
            Y = 715,
            Tint = Color.White
        };

        private void CreateStatsPanel()
        {
            var panel = new Container
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                X = -30,
                Y = 30,
                Size = new ScalableVector2(320, 210)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "LIVE STATS", 18)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Tint = Color.White
            };

            for (var i = 0; i < 8; i++)
            {
                StatLines.Add(new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"), string.Empty, 16)
                {
                    Parent = panel,
                    Alignment = Alignment.TopLeft,
                    Y = 30 + i * 21,
                    Tint = StatTextColor
                });
            }
        }

        public override void Update(GameTime gameTime)
        {
            UpdateScreenScrollContainerSize();

            if (KeyboardManager.IsUniqueKeyPress(Keys.D1) || KeyboardManager.IsUniqueKeyPress(Keys.NumPad1))
                ToggleNewButtons();

            if (KeyboardManager.IsUniqueKeyPress(Keys.D2) || KeyboardManager.IsUniqueKeyPress(Keys.NumPad2))
                ToggleOldButtons();

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
                ResetButtons();

            StatsRefreshTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (StatsRefreshTimer >= StatsRefreshTime)
            {
                StatsRefreshTimer = 0;
                UpdateStatsText();
            }

            Container?.Update(gameTime);
            UpdateScrollButtonVisibility();
        }

        private void UpdateScreenScrollContainerSize()
        {
            var width = Container.Width;
            var height = Container.Height;
            var contentHeight = Math.Max(ContentHeight, height);

            if (ScreenScrollContainer.Width != width)
                ScreenScrollContainer.Width = width;

            if (ScreenScrollContainer.Height != height)
                ScreenScrollContainer.Height = height;

            if (ContentContainer.Width != width)
                ContentContainer.Width = width;

            if (ContentContainer.Height != contentHeight)
                ContentContainer.Height = contentHeight;
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();

        private void ToggleNewButtons()
        {
            NewButtonGroup.Visible = !NewButtonGroup.Visible;
            UpdateStateText();
        }

        private void ToggleOldButtons()
        {
            OldButtonGroup.Visible = !OldButtonGroup.Visible;
            UpdateStateText();
        }

        private void ResetButtons()
        {
            NewButtonGroup.Visible = true;
            OldButtonGroup.Visible = true;
            UpdateStateText();
        }

        private void UpdateStateText()
        {
            NewStateText.Text = $"New buttons: {(NewButtonGroup.Visible ? "ON" : "OFF")}";
            NewStateText.Tint = NewButtonGroup.Visible ? EnabledColor : DisabledColor;

            OldStateText.Text = $"Old image buttons: {(OldButtonGroup.Visible ? "ON" : "OFF")}";
            OldStateText.Tint = OldButtonGroup.Visible ? EnabledColor : DisabledColor;
        }

        private void UpdateScrollButtonVisibility()
        {
            foreach (var scrollContainer in ButtonScrollContainers)
                UpdateScrollButtonVisibility(scrollContainer);
        }

        private static void UpdateScrollButtonVisibility(ScrollContainer scrollContainer)
        {
            foreach (var drawable in scrollContainer.ContentContainer.Children)
                drawable.Visible = !RectangleF.Intersection(drawable.ScreenRectangle, scrollContainer.ScreenRectangle).IsEmpty;
        }

        private void UpdateStatsText()
        {
            StatLines[0].Text = $"FPS / UPS: {PerformanceStats.FrameRate} / {PerformanceStats.UpdateRate}";
            StatLines[1].Text = $"Frame: {PerformanceStats.FrameTimeMs:0.00} ms avg {PerformanceStats.AverageFrameTimeMs:0.00} ms";
            StatLines[2].Text = $"Draw: {PerformanceStats.DrawTimeMs:0.00} ms avg {PerformanceStats.AverageDrawTimeMs:0.00} ms";
            StatLines[3].Text = $"Screen draw: {PerformanceStats.ScreenDrawTimeMs:0.00} ms";
            StatLines[4].Text = $"Drawn drawables: {PerformanceStats.DrawnDrawableCount}";
            StatLines[5].Text = $"New group: {(NewButtonGroup.Visible ? "visible" : "hidden")}";
            StatLines[6].Text = $"Old group: {(OldButtonGroup.Visible ? "visible" : "hidden")}";
            StatLines[7].Text = $"Buttons per group: {ButtonCount} ({VisibleButtonCount} visible)";
        }
    }
}
