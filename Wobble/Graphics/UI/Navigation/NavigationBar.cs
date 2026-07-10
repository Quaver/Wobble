using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;

namespace Wobble.Graphics.UI.Navigation
{
    public enum NavigationBarRegion
    {
        Left,
        Center,
        Right
    }

    public enum NavigationBarBorderPosition
    {
        Top,
        Bottom
    }

    /// <summary>
    ///     Options for the animated line drawn along an edge of a <see cref="NavigationBar"/>.
    /// </summary>
    public class NavigationBarBorderOptions
    {
        public bool Enabled { get; set; }

        public NavigationBarBorderPosition Position { get; set; } = NavigationBarBorderPosition.Bottom;

        public Color BorderColor { get; set; } = Color.White;

        public Color AnimatedBorderColor { get; set; } = Color.White;

        public float Thickness { get; set; } = 2;

        public float AnimatedBorderWidth { get; set; } = 150;

        /// <summary>
        ///     Time in milliseconds for the smaller line to travel from one side to the other.
        /// </summary>
        public int AnimationDuration { get; set; } = 15000;
    }

    /// <summary>
    ///     Options used by <see cref="NavigationBar.AddRoundedButton"/>.
    /// </summary>
    public class NavigationBarButtonOptions
    {
        public Texture2D Icon { get; set; }

        public Vector2? IconSize { get; set; }

        public string Text { get; set; }

        public WobbleFontStore Font { get; set; }

        public int FontSize { get; set; } = 16;

        public Color ForegroundColor { get; set; } = Color.White;

        public Color BackgroundColor { get; set; } = Color.White;

        public float Width { get; set; } = 120;

        public float Height { get; set; } = 40;

        public ButtonSizeMode WidthMode { get; set; } = ButtonSizeMode.Fixed;

        public ButtonSizeMode HeightMode { get; set; } = ButtonSizeMode.Fixed;

        public Vector2 AutoSizePadding { get; set; } = new Vector2(32, 12);

        public float? CornerRadius { get; set; }

        public bool PerformHoverFade { get; set; } = true;

        public EventHandler ClickAction { get; set; }
    }

    /// <summary>
    ///     A reusable navigation bar that lays out arbitrary drawables in left, center, and right regions.
    /// </summary>
    public class NavigationBar : Sprite
    {
        private readonly Dictionary<NavigationBarRegion, List<Drawable>> _regions =
            new Dictionary<NavigationBarRegion, List<Drawable>>
            {
                { NavigationBarRegion.Left, new List<Drawable>() },
                { NavigationBarRegion.Center, new List<Drawable>() },
                { NavigationBarRegion.Right, new List<Drawable>() }
            };

        private readonly Dictionary<Drawable, NavigationBarRegion> _items =
            new Dictionary<Drawable, NavigationBarRegion>();

        private bool _initialized;
        private float _edgePadding = 24;
        private float _itemSpacing = 12;
        private float _layoutWidth;
        private float _layoutHeight;

        private NavigationBarBorderOptions BorderOptions { get; }

        public Sprite BorderLine { get; private set; }

        public Sprite AnimatedBorderLine { get; private set; }

        public Color BackgroundColor
        {
            get => Tint;
            set => Tint = value;
        }

        public float EdgePadding
        {
            get => _edgePadding;
            set
            {
                value = Math.Max(0, value);

                if (Math.Abs(_edgePadding - value) < float.Epsilon)
                    return;

                _edgePadding = value;
                RefreshLayout();
            }
        }

        public float ItemSpacing
        {
            get => _itemSpacing;
            set
            {
                value = Math.Max(0, value);

                if (Math.Abs(_itemSpacing - value) < float.Epsilon)
                    return;

                _itemSpacing = value;
                RefreshLayout();
            }
        }

        public NavigationBar(float width, float height, Color? backgroundColor = null,
            NavigationBarBorderOptions borderOptions = null)
        {
            Image = WobbleAssets.WhiteBox;
            Size = new ScalableVector2(width, height);
            Tint = backgroundColor ?? Color.Transparent;
            _layoutWidth = Width;
            _layoutHeight = Height;
            BorderOptions = borderOptions;
            CreateAnimatedBorder();
            _initialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            PerformBorderAnimation();
            base.Update(gameTime);
        }

        public void Add(NavigationBarRegion region, Drawable drawable)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));

            if (_items.TryGetValue(drawable, out var existingRegion))
            {
                if (existingRegion != region)
                {
                    _regions[existingRegion].Remove(drawable);
                    _regions[region].Add(drawable);
                    _items[drawable] = region;
                }

                RefreshLayout();
                return;
            }

            _regions[region].Add(drawable);
            _items.Add(drawable, region);
            drawable.Parent = this;
            RefreshLayout();
        }

        public bool Remove(Drawable drawable)
        {
            if (drawable == null || !_items.TryGetValue(drawable, out var region))
                return false;

            (drawable as Button)?.ResetInteractionState();
            _regions[region].Remove(drawable);
            _items.Remove(drawable);
            DetachWithoutDestroying(drawable);
            RefreshLayout();
            return true;
        }

        public void Clear(NavigationBarRegion? region = null)
        {
            var drawables = region == null
                ? _items.Keys.ToArray()
                : _regions[region.Value].ToArray();

            foreach (var drawable in drawables)
                Remove(drawable);
        }

        public RoundedButton AddRoundedButton(NavigationBarRegion region, NavigationBarButtonOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (!string.IsNullOrEmpty(options.Text) && options.Font == null)
                throw new ArgumentException("A font is required when button text is supplied.", nameof(options));

            if (!string.IsNullOrEmpty(options.Text) && options.FontSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Font size must be greater than zero.");

            var button = new RoundedButton(options.ClickAction)
            {
                Size = new ScalableVector2(options.Width, options.Height),
                WidthMode = options.WidthMode,
                HeightMode = options.HeightMode,
                AutoSizePadding = options.AutoSizePadding,
                CornerRadius = options.CornerRadius,
                PerformHoverFade = options.PerformHoverFade,
                Tint = options.BackgroundColor
            };

            if (options.Icon != null)
            {
                button.SetIcon(options.Icon, options.IconSize);
                button.Icon.Tint = options.ForegroundColor;
            }

            if (!string.IsNullOrEmpty(options.Text))
                button.SetLabel(options.Font, options.Text, options.FontSize, options.ForegroundColor);

            button.RecalculateAutoSize();
            Add(region, button);
            return button;
        }

        public void RefreshLayout()
        {
            if (!_initialized)
                return;

            RemoveDisposedItems();
            LayoutLeftItems();
            LayoutCenterItems();
            LayoutRightItems();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (!_initialized)
                return;

            if (Math.Abs(_layoutWidth - Width) < float.Epsilon &&
                Math.Abs(_layoutHeight - Height) < float.Epsilon)
                return;

            _layoutWidth = Width;
            _layoutHeight = Height;
            RefreshBorderLayout();
            RefreshLayout();
        }

        private void CreateAnimatedBorder()
        {
            if (BorderOptions?.Enabled != true)
                return;

            var thickness = Math.Max(0, BorderOptions.Thickness);
            var animatedWidth = Math.Min(Width, Math.Max(0, BorderOptions.AnimatedBorderWidth));
            var alignment = BorderOptions.Position == NavigationBarBorderPosition.Top
                ? Alignment.TopLeft
                : Alignment.BotLeft;

            BorderLine = new Sprite
            {
                Parent = this,
                Image = WobbleAssets.WhiteBox,
                Size = new ScalableVector2(Width, thickness),
                Alignment = alignment,
                Tint = BorderOptions.BorderColor
            };

            AnimatedBorderLine = new Sprite
            {
                Parent = BorderLine,
                Image = WobbleAssets.WhiteBox,
                Size = new ScalableVector2(animatedWidth, thickness),
                Tint = BorderOptions.AnimatedBorderColor,
                X = BorderOptions.Position == NavigationBarBorderPosition.Bottom ? Width - animatedWidth : 0
            };
        }

        private void RefreshBorderLayout()
        {
            if (BorderLine == null)
                return;

            BorderLine.Width = Width;
            AnimatedBorderLine.ClearAnimations();
            AnimatedBorderLine.Width = Math.Min(Width, Math.Max(0, BorderOptions.AnimatedBorderWidth));
            AnimatedBorderLine.X = Math.Min(Math.Max(0, AnimatedBorderLine.X), Width - AnimatedBorderLine.Width);
        }

        private void PerformBorderAnimation()
        {
            if (AnimatedBorderLine == null || AnimatedBorderLine.Animations.Count != 0)
                return;

            var rightEdge = Width - AnimatedBorderLine.Width;
            var target = AnimatedBorderLine.X > rightEdge / 2f ? 0 : rightEdge;
            AnimatedBorderLine.MoveToX(target, Easing.Linear, Math.Max(1, BorderOptions.AnimationDuration));
        }

        private void LayoutLeftItems()
        {
            var offset = EdgePadding;

            foreach (var drawable in _regions[NavigationBarRegion.Left])
            {
                drawable.Alignment = Alignment.MidLeft;
                drawable.X = offset;
                drawable.Y = 0;
                offset += drawable.Width + ItemSpacing;
            }
        }

        private void LayoutCenterItems()
        {
            var items = _regions[NavigationBarRegion.Center];
            var totalWidth = GetTotalWidth(items);
            var offset = -totalWidth / 2f;

            foreach (var drawable in items)
            {
                drawable.Alignment = Alignment.MidCenter;
                drawable.X = offset + drawable.Width / 2f;
                drawable.Y = 0;
                offset += drawable.Width + ItemSpacing;
            }
        }

        private void LayoutRightItems()
        {
            var items = _regions[NavigationBarRegion.Right];
            var totalWidth = GetTotalWidth(items);
            var offset = 0f;

            foreach (var drawable in items)
            {
                drawable.Alignment = Alignment.MidRight;
                drawable.X = -EdgePadding - totalWidth + offset + drawable.Width;
                drawable.Y = 0;
                offset += drawable.Width + ItemSpacing;
            }
        }

        private float GetTotalWidth(IReadOnlyCollection<Drawable> items)
        {
            if (items.Count == 0)
                return 0;

            return items.Sum(x => x.Width) + ItemSpacing * (items.Count - 1);
        }

        private void RemoveDisposedItems()
        {
            foreach (var drawable in _items.Keys.Where(x => x.IsDisposed).ToArray())
            {
                var region = _items[drawable];
                _regions[region].Remove(drawable);
                _items.Remove(drawable);
            }
        }

        private static void DetachWithoutDestroying(Drawable drawable)
        {
            var destroyIfParentIsNull = drawable.DestroyIfParentIsNull;
            drawable.DestroyIfParentIsNull = false;
            drawable.Parent = null;
            drawable.DestroyIfParentIsNull = destroyIfParentIsNull;
        }
    }
}
