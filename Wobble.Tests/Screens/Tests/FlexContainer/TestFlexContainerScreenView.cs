using System;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;
using FlexLayout = Wobble.Graphics.FlexContainer;

namespace Wobble.Tests.Screens.Tests.FlexContainer
{
    public class TestFlexContainerScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(17, 24, 32);
        private static readonly Color PanelColor = new Color(27, 37, 48);
        private static readonly Color MutedColor = new Color(155, 170, 188);
        private static readonly Color[] ItemColors =
        {
            new Color(30, 144, 255),
            new Color(117, 92, 222),
            new Color(22, 163, 74),
            new Color(234, 88, 12),
            new Color(219, 39, 119),
            new Color(8, 145, 178)
        };

        private readonly float[] _gapValues = { 0, 8, 20, 36 };
        private int _gapIndex = 1;
        private readonly FlexLayout _toolbar;
        private readonly FlexLayout _playground;
        private readonly SpriteTextPlus _status;

        public TestFlexContainerScreenView(Screen screen) : base(screen)
        {
            AddText(Container, "FLEX CONTAINER", 26, 24, Alignment.TopCenter, Color.White, "inter-bold");
            AddText(Container,
                "Cycle every flexbox mode, then resize the window to verify automatic wrapping and relayout.",
                15, 58, Alignment.TopCenter, MutedColor, "inter-regular");

            _playground = new FlexLayout
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 210,
                Size = new ScalableVector2(Math.Max(1, Container.Width - 80), Math.Max(160, Container.Height - 240)),
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustifyContent.FlexStart,
                AlignItems = FlexAlignItems.Stretch,
                AlignContent = FlexAlignContent.Stretch,
                Gap = _gapValues[_gapIndex]
            };
            _playground.AddBorder(new Color(83, 105, 130), 2);

            _toolbar = new FlexLayout
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 88,
                Size = new ScalableVector2(Math.Max(1, Container.Width - 60), 86),
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.FlexStart,
                AlignContent = FlexAlignContent.FlexStart,
                Gap = 8
            };

            CreateModeButton("Direction", () => _playground.Direction,
                value => _playground.Direction = NextEnum(value));
            CreateModeButton("Wrap", () => _playground.Wrap,
                value => _playground.Wrap = NextEnum(value));
            CreateModeButton("Justify", () => _playground.JustifyContent,
                value => _playground.JustifyContent = NextEnum(value));
            CreateModeButton("Items", () => _playground.AlignItems,
                value => _playground.AlignItems = NextEnum(value));
            CreateModeButton("Content", () => _playground.AlignContent,
                value => _playground.AlignContent = NextEnum(value));
            CreateGapButton();

            _status = AddText(Container, string.Empty, 14, 180, Alignment.TopCenter, MutedColor,
                "inter-semibold");

            CreatePlaygroundItems();
            UpdateStatus();
        }

        private void CreatePlaygroundItems()
        {
            var first = CreateItem("A\ngrow 1 · basis 130", 130, 64, ItemColors[0]);
            _playground.SetItemOptions(first, new FlexItemOptions { Basis = 130, Grow = 1 });

            var second = CreateItem("B\nshrink 0 · basis 180", 180, 92, ItemColors[1]);
            _playground.SetItemOptions(second, new FlexItemOptions { Basis = 180, Shrink = 0 });

            var third = CreateItem("C\norder -1 · grow 2", 100, 74, ItemColors[2]);
            _playground.SetItemOptions(third, new FlexItemOptions { Basis = 100, Grow = 2, Order = -1 });

            var fourth = CreateItem("D\nalign-self center", 150, 58, ItemColors[3]);
            _playground.SetItemOptions(fourth,
                new FlexItemOptions { Basis = 150, AlignSelf = FlexAlignSelf.Center });

            var fifth = CreateItem("E\nshrink 2 · basis 220", 220, 82, ItemColors[4]);
            _playground.SetItemOptions(fifth, new FlexItemOptions { Basis = 220, Shrink = 2 });

            var nestedHost = CreateItem("NESTED FLEX", 190, 104, PanelColor);
            var nested = new FlexLayout
            {
                Parent = nestedHost,
                Alignment = Alignment.BotCenter,
                Y = -9,
                Size = new ScalableVector2(166, 55),
                Direction = FlexDirection.Row,
                JustifyContent = FlexJustifyContent.SpaceEvenly,
                AlignItems = FlexAlignItems.Center,
                Gap = 5
            };
            nested.AddBorder(new Color(130, 150, 172));
            CreateNestedItem(nested, "1", ItemColors[5]);
            CreateNestedItem(nested, "2", ItemColors[0]);
            CreateNestedItem(nested, "3", ItemColors[2]);
            _playground.SetItemOptions(nestedHost,
                new FlexItemOptions { Basis = 190, Grow = 1, AlignSelf = FlexAlignSelf.Stretch });
        }

        private Container CreateItem(string label, float width, float height, Color color)
        {
            var item = new Container
            {
                Parent = _playground,
                Size = new ScalableVector2(width, height)
            };

            new Sprite
            {
                Parent = item,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(0, 0, 1, 1),
                Image = WobbleAssets.WhiteBox,
                Tint = color
            };

            AddText(item, label, 14, 0, Alignment.MidCenter, Color.White, "inter-semibold");
            return item;
        }

        private static void CreateNestedItem(FlexLayout parent, string label, Color color)
        {
            var item = new Container { Parent = parent, Size = new ScalableVector2(38, 34) };
            new Sprite
            {
                Parent = item,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(0, 0, 1, 1),
                Image = WobbleAssets.WhiteBox,
                Tint = color
            };
            AddText(item, label, 13, 0, Alignment.MidCenter, Color.White, "inter-bold");
        }

        private void CreateModeButton<T>(string prefix, Func<T> getValue, Action<T> setValue) where T : struct
        {
            var button = CreateToolbarButton(prefix + ": " + getValue());
            button.Clicked += (sender, args) =>
            {
                setValue(getValue());
                button.Label.Text = prefix + ": " + getValue();
                UpdateStatus();
            };
        }

        private void CreateGapButton()
        {
            var button = CreateToolbarButton("Gap: " + _gapValues[_gapIndex]);
            button.Clicked += (sender, args) =>
            {
                _gapIndex = (_gapIndex + 1) % _gapValues.Length;
                _playground.Gap = _gapValues[_gapIndex];
                button.Label.Text = "Gap: " + _gapValues[_gapIndex];
                UpdateStatus();
            };
        }

        private RoundedButton CreateToolbarButton(string label)
        {
            var button = new RoundedButton
            {
                Parent = _toolbar,
                Size = new ScalableVector2(188, 36),
                Tint = new Color(48, 63, 79),
                CornerRadius = 4,
                AntiAliasedEdges = false
            };
            button.SetLabel(FontManager.GetWobbleFont("inter-semibold"), label, 13, Color.White);
            return button;
        }

        private void UpdateStatus()
        {
            if (_status == null || _playground == null)
                return;

            _status.Text = $"{_playground.Direction} · {_playground.Wrap} · " +
                           $"justify {_playground.JustifyContent} · items {_playground.AlignItems} · " +
                           $"content {_playground.AlignContent} · gap {_playground.Gap:0}";
        }

        private static T NextEnum<T>(T value) where T : struct
        {
            var values = (T[]) Enum.GetValues(typeof(T));
            var index = Array.IndexOf(values, value);
            return values[(index + 1) % values.Length];
        }

        private static SpriteTextPlus AddText(Drawable parent, string text, int size, float y,
            Alignment alignment, Color color, string font) => new SpriteTextPlus(
            FontManager.GetWobbleFont(font), text, size)
        {
            Parent = parent,
            Alignment = alignment,
            Y = y,
            Tint = color,
            TextAlignment = TextAlignment.Center
        };

        public override void Update(GameTime gameTime)
        {
            var toolbarWidth = Math.Max(1, Container.Width - 60);
            if (Math.Abs(_toolbar.Width - toolbarWidth) > 0.001f)
                _toolbar.Width = toolbarWidth;

            var playgroundWidth = Math.Max(1, Container.Width - 80);
            var playgroundHeight = Math.Max(160, Container.Height - 240);
            if (Math.Abs(_playground.Width - playgroundWidth) > 0.001f ||
                Math.Abs(_playground.Height - playgroundHeight) > 0.001f)
            {
                _playground.Size = new ScalableVector2(playgroundWidth, playgroundHeight);
            }

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
