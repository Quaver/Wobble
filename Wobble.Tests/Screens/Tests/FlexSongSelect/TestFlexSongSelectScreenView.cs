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
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using FlexLayout = Wobble.Graphics.FlexContainer;

namespace Wobble.Tests.Screens.Tests.FlexSongSelect
{
    public class TestFlexSongSelectScreenView : ScreenView
    {
        private const float RowHeight = 96;
        private const float RowGap = 12;
        private const float RowLeftPadding = 20;
        private const float InactiveRowScale = 0.96f;
        private const int MapsetCount = 30;

        private static readonly Color BackgroundColor = new Color(25, 26, 28);
        private static readonly Color RowColor = new Color(34, 35, 37);
        private static readonly Color BorderColor = new Color(0, 151, 232);
        private static readonly Color BlueColor = new Color(0, 139, 218);
        private static readonly Color MutedColor = new Color(139, 143, 148);
        private static readonly Color CoverColor = new Color(215, 219, 224);

        private static readonly string[] Titles =
        {
            "Bumble Bee",
            "Goodbye Mr A",
            "Across The Solar System",
            "Weeble Wobble (Nightcore Remix)",
            "I'll Fight Back",
            "Ipace",
            "SOFT, SPIKE",
            "Alone Again (Naturally)",
            "Worlds Collide",
            "Starlight Memory"
        };

        private static readonly string[] Artists =
        {
            "Bambee",
            "The Hoosiers",
            "LV.4 mixed by Rems",
            "Eliminate",
            "Sullivan King",
            "INABAKUMORI",
            "Frums",
            "Gilbert O'Sullivan",
            "Teminite",
            "Sakuzyo"
        };

        private static readonly string[] Creators =
        {
            "Warp9000", "Cuckson", "neyoru", "Rellorel", "AiAe",
            "7xbi", "Swirrl", "Hekireki", "Evening", "Kuro"
        };

        private readonly List<Container> _rowShells = new List<Container>();
        private readonly ScrollContainer _scrollContainer;
        private readonly Container _mapsetArea;
        private int _selectedIndex;

        public TestFlexSongSelectScreenView(Screen screen) : base(screen)
        {
            AddText(Container, "FLEX SONG SELECT", 24, 18, Alignment.TopCenter, Color.White, "inter-bold");
            AddText(Container,
                "Each mapset is a nested flex row. Use Up/Down to select, or scroll to inspect the list.",
                14, 50, Alignment.TopCenter, MutedColor, "inter-regular");

            var viewportWidth = Math.Max(1, Container.Width - 80);
            var viewportHeight = Math.Max(1, Container.Height - 92);
            var contentHeight = MapsetCount * RowHeight + (MapsetCount - 1) * RowGap;

            _scrollContainer = new ScrollContainer(
                new ScalableVector2(viewportWidth, viewportHeight),
                new ScalableVector2(viewportWidth, contentHeight))
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 78,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                ScrollSpeed = 80,
                Tint = Color.Transparent
            };
            _scrollContainer.Scrollbar.Width = 5;
            _scrollContainer.Scrollbar.Tint = new Color(105, 108, 112);

            _mapsetArea = new Container
            {
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(viewportWidth - 18, contentHeight)
            };

            for (var i = 0; i < MapsetCount; i++)
                CreateMapsetRow(i);

            _scrollContainer.AddContainedDrawable(_mapsetArea);
            ApplySharedSpriteBatch(_mapsetArea);
            ApplySelection(false);
        }

        private void CreateMapsetRow(int index)
        {
            var shell = new Container(
                new ScalableVector2(_mapsetArea.Width, RowHeight),
                new ScalableVector2(0, index * (RowHeight + RowGap)))
            {
                Parent = _mapsetArea
            };
            _rowShells.Add(shell);

            new Sprite
            {
                Parent = shell,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(0, 0, 1, 1),
                Image = WobbleAssets.WhiteBox,
                Tint = RowColor
            };

            var row = new FlexLayout
            {
                Parent = shell,
                Alignment = Alignment.TopLeft,
                X = RowLeftPadding,
                Size = new ScalableVector2(Math.Max(1, shell.Width - RowLeftPadding), shell.Height),
                Direction = FlexDirection.Row,
                JustifyContent = FlexJustifyContent.FlexStart,
                AlignItems = FlexAlignItems.Stretch,
                Gap = 20
            };
            shell.SizeChanged += (sender, args) => row.Size =
                new ScalableVector2(Math.Max(1, shell.Width - RowLeftPadding), shell.Height);

            var info = new FlexLayout
            {
                Parent = row,
                Direction = FlexDirection.Column,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.FlexStart,
                Gap = 4,
                Size = new ScalableVector2(430, RowHeight)
            };
            row.SetItemOptions(info, new FlexItemOptions { Basis = 430, Grow = 1, Shrink = 1 });

            var title = Titles[index % Titles.Length];
            var artist = Artists[index % Artists.Length];
            var creator = Creators[index % Creators.Length];
            var titleText = AddText(info, title, 21, 0, Alignment.TopLeft, new Color(218, 218, 220),
                "inter-bold");
            var metadata = AddText(info, $"{artist}  |  By: {creator}", 15, 0, Alignment.TopLeft, BlueColor,
                "inter-semibold");
            info.SetItemOptions(titleText, new FlexItemOptions { Shrink = 0 });
            info.SetItemOptions(metadata, new FlexItemOptions { Shrink = 0 });

            var modeBadge = CreateBadge(row, index % 3 == 0 ? "7K" : "4K", 82, BlueColor);
            row.SetItemOptions(modeBadge,
                new FlexItemOptions { Basis = 82, Shrink = 0, AlignSelf = FlexAlignSelf.Center });

            var ranked = index % 4 != 0;
            var unsubmitted = index % 7 == 5;
            var status = unsubmitted ? "UNSUBMITTED" : ranked ? "RANKED" : "UNRANKED";
            var statusColor = unsubmitted
                ? new Color(104, 104, 106)
                : ranked ? new Color(28, 147, 91) : new Color(205, 79, 80);
            var statusBadge = CreateBadge(row, status, 132, statusColor);
            row.SetItemOptions(statusBadge,
                new FlexItemOptions { Basis = 132, Shrink = 0, AlignSelf = FlexAlignSelf.Center });

            var cover = new Sprite
            {
                Parent = row,
                Size = new ScalableVector2(410, RowHeight),
                Image = WobbleAssets.WhiteBox,
                Tint = index % 2 == 0 ? CoverColor : new Color(180, 187, 195)
            };
            row.SetItemOptions(cover, new FlexItemOptions { Basis = 410, Shrink = 1 });
            shell.AddBorder(BorderColor, 2);
        }

        private static RoundedButton CreateBadge(Drawable parent, string label, float width, Color color)
        {
            var badge = new RoundedButton
            {
                Parent = parent,
                Size = new ScalableVector2(width, 34),
                Tint = color,
                AntiAliasedEdges = false,
                PerformHoverFade = false,
                IsClickable = false
            };
            badge.SetLabel(FontManager.GetWobbleFont("inter-semibold"), label, 13, Color.White);
            return badge;
        }

        private static SpriteTextPlus AddText(Drawable parent, string text, int size, float y,
            Alignment alignment, Color color, string font) => new SpriteTextPlus(
            FontManager.GetWobbleFont(font), text, size)
        {
            Parent = parent,
            Alignment = alignment,
            Y = y,
            Tint = color
        };

        private static void ApplySharedSpriteBatch(Drawable drawable)
        {
            drawable.UsePreviousSpriteBatchOptions = true;
            drawable.SetChildrenVisibility = true;
            foreach (var child in drawable.Children)
                ApplySharedSpriteBatch(child);
        }

        private void UpdateResponsiveLayout()
        {
            var viewportWidth = Math.Max(1, Container.Width - 80);
            var viewportHeight = Math.Max(1, Container.Height - 92);
            if (Math.Abs(_scrollContainer.Width - viewportWidth) > 0.001f ||
                Math.Abs(_scrollContainer.Height - viewportHeight) > 0.001f)
            {
                _scrollContainer.Size = new ScalableVector2(viewportWidth, viewportHeight);
                _scrollContainer.ContentContainer.Width = viewportWidth;
                _mapsetArea.Width = Math.Max(1, viewportWidth - 18);
                foreach (var row in _rowShells)
                    row.Width = _mapsetArea.Width;
            }
        }

        private void UpdateRowVisibility()
        {
            var viewport = _scrollContainer.ScreenRectangle;
            foreach (var row in _rowShells)
            {
                var visible = RectangleF.Intersects(row.ScreenRectangle, viewport);
                if (row.Visible != visible)
                    row.Visible = visible;
            }
        }

        private void UpdateSelectionInput()
        {
            var nextIndex = _selectedIndex;
            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
                nextIndex = Math.Max(0, _selectedIndex - 1);
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
                nextIndex = Math.Min(_rowShells.Count - 1, _selectedIndex + 1);

            if (nextIndex == _selectedIndex)
                return;

            _selectedIndex = nextIndex;
            ApplySelection(true);
        }

        private void ApplySelection(bool keepVisible)
        {
            for (var i = 0; i < _rowShells.Count; i++)
            {
                var targetScale = i == _selectedIndex ? Vector2.One : new Vector2(InactiveRowScale);
                if (_rowShells[i].Scale != targetScale)
                    _rowShells[i].Scale = targetScale;
            }

            if (!keepVisible || _rowShells.Count == 0)
                return;

            var rowTop = _selectedIndex * (RowHeight + RowGap);
            var visibleTop = -_scrollContainer.TargetY;
            var visibleBottom = visibleTop + _scrollContainer.Height;
            var targetY = _scrollContainer.TargetY;

            if (rowTop < visibleTop)
                targetY = -rowTop;
            else if (rowTop + RowHeight > visibleBottom)
                targetY = -(rowTop + RowHeight - _scrollContainer.Height);

            if (Math.Abs(targetY - _scrollContainer.TargetY) > 0.001f)
                _scrollContainer.ScrollTo(targetY, 150);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateResponsiveLayout();
            UpdateSelectionInput();
            Container?.Update(gameTime);
            UpdateRowVisibility();
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
