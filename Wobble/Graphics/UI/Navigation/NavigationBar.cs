using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

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

    public enum NavigationBarBackgroundType
    {
        SolidColor,
        Image,
        Gradient
    }

    public enum NavigationBarImageFit
    {
        Stretch,
        Cover,
        Contain,
        Tile
    }

    public enum NavigationBarGradientType
    {
        Linear,
        Radial
    }

    public class NavigationBarGradientStop
    {
        public float Position { get; set; }

        public Color Color { get; set; } = Color.White;
    }

    public class NavigationBarGradientOptions
    {
        public NavigationBarGradientType Type { get; set; } = NavigationBarGradientType.Linear;

        public IReadOnlyList<NavigationBarGradientStop> Stops { get; set; }

        /// <summary>
        ///     The direction of a linear gradient in degrees. Zero points from left to right and 90 points
        ///     from top to bottom.
        /// </summary>
        public float AngleDegrees { get; set; }

        /// <summary>
        ///     The normalized origin of a radial gradient.
        /// </summary>
        public Vector2 RadialOrigin { get; set; } = new Vector2(0.5f, 0.5f);

        /// <summary>
        ///     The radius of a radial gradient relative to the distance from its origin to the farthest corner.
        /// </summary>
        public float RadialRadius { get; set; } = 1;
    }

    public class NavigationBarBackgroundOptions
    {
        public NavigationBarBackgroundType Type { get; set; } = NavigationBarBackgroundType.SolidColor;

        public Color SolidColor { get; set; } = Color.Transparent;

        public Texture2D Image { get; set; }

        public NavigationBarImageFit ImageFit { get; set; } = NavigationBarImageFit.Stretch;

        public NavigationBarGradientOptions Gradient { get; set; }
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

        public bool AntiAliasedEdges { get; set; } = true;

        public bool ExpandLabelOnHover { get; set; }

        public bool AlwaysShowLabel { get; set; }

        public int HoverExpansionDuration { get; set; } = 150;

        public float ExpandedLabelRightPadding { get; set; }

        public EventHandler ClickAction { get; set; }

        public IReadOnlyList<NavigationBarDropdownOption> DropdownOptions { get; set; }

        public Color? DropdownBackgroundColor { get; set; }

        public Color? DropdownItemBackgroundColor { get; set; }

        public Color? DropdownForegroundColor { get; set; }

        public float DropdownItemHeight { get; set; } = 32;

        public Vector2 DropdownItemPadding { get; set; } = new Vector2(20, 0);

        public float DropdownPadding { get; set; } = 4;

        public float DropdownItemSpacing { get; set; } = 2;
    }

    /// <summary>
    ///     A text option displayed in a navigation bar button's dropdown.
    /// </summary>
    public class NavigationBarDropdownOption
    {
        public string Text { get; set; }

        public EventHandler ClickAction { get; set; }
    }

    /// <summary>
    ///     A reusable navigation bar that lays out arbitrary drawables in left, center, and right regions.
    /// </summary>
    public class NavigationBar : Sprite
    {
        // Gradients are sampled in normalized coordinates, so a capped texture can be stretched to
        // the render rectangle without changing its geometry. Bucketing also avoids regeneration for
        // every individual pixel while a window is being resized.
        private const int GradientTextureMaximumDimension = 512;
        private const int GradientTextureSizeBucket = 8;

        private readonly Dictionary<NavigationBarRegion, List<Drawable>> _regions =
            new Dictionary<NavigationBarRegion, List<Drawable>>
            {
                { NavigationBarRegion.Left, new List<Drawable>() },
                { NavigationBarRegion.Center, new List<Drawable>() },
                { NavigationBarRegion.Right, new List<Drawable>() }
            };

        private readonly Dictionary<Drawable, NavigationBarRegion> _items =
            new Dictionary<Drawable, NavigationBarRegion>();

        private readonly Dictionary<Drawable, Vector2> _itemSizes = new Dictionary<Drawable, Vector2>();

        private bool _initialized;
        private bool _refreshingBackground;
        private float _edgePadding = 24;
        private float _itemSpacing = 12;
        private float _layoutWidth;
        private float _layoutHeight;

        private DropdownState _openDropdown;
        private NavigationBarBackgroundOptions _background;
        private Texture2D _generatedGradientTexture;

        private NavigationBarBorderOptions BorderOptions { get; }

        public Sprite BorderLine { get; private set; }

        public Sprite AnimatedBorderLine { get; private set; }

        public Color BackgroundColor
        {
            get => Background?.Type == NavigationBarBackgroundType.SolidColor
                ? Background.SolidColor
                : Color.Transparent;
            set => Background = new NavigationBarBackgroundOptions
            {
                Type = NavigationBarBackgroundType.SolidColor,
                SolidColor = value
            };
        }

        public NavigationBarBackgroundOptions Background
        {
            get => _background;
            set
            {
                _background = value ?? CreateTransparentBackground();
                RefreshBackground();
            }
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
            Background = new NavigationBarBackgroundOptions
            {
                Type = NavigationBarBackgroundType.SolidColor,
                SolidColor = backgroundColor ?? Color.Transparent
            };
            _layoutWidth = Width;
            _layoutHeight = Height;
            BorderOptions = borderOptions;
            CreateAnimatedBorder();
            _initialized = true;
        }

        /// <summary>
        ///     Reapplies the current background options after they have been changed in place.
        /// </summary>
        public void RefreshBackground()
        {
            if (_refreshingBackground)
                return;

            _refreshingBackground = true;

            try
            {
                DisposeGeneratedGradient();

                switch (Background.Type)
                {
                    case NavigationBarBackgroundType.SolidColor:
                        Image = WobbleAssets.WhiteBox;
                        Tint = Background.SolidColor;
                        break;
                    case NavigationBarBackgroundType.Image:
                        if (Background.Image == null || Background.Image.IsDisposed ||
                            !Enum.IsDefined(typeof(NavigationBarImageFit), Background.ImageFit))
                        {
                            ApplyTransparentBackground();
                            break;
                        }

                        Image = Background.Image;
                        Tint = Color.White;
                        break;
                    case NavigationBarBackgroundType.Gradient:
                        if (!TryCreateGradientTexture(Background.Gradient, out var texture))
                        {
                            ApplyTransparentBackground();
                            break;
                        }

                        _generatedGradientTexture = texture;
                        Image = texture;
                        Tint = Color.White;
                        break;
                    default:
                        ApplyTransparentBackground();
                        break;
                }
            }
            finally
            {
                _refreshingBackground = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            PerformBorderAnimation();
            base.Update(gameTime);

            if (_openDropdown != null)
            {
                PositionDropdown(_openDropdown);

                if (MouseManager.IsUniqueClick(MouseButton.Left) &&
                    !_openDropdown.Trigger.IsHovered() && !_openDropdown.Menu.IsHovered())
                    CloseDropdown();
            }

            if (HaveItemSizesChanged())
                RefreshLayout();
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

        public bool Remove(Drawable drawable, bool destroy = false)
        {
            if (drawable == null || !_items.TryGetValue(drawable, out var region))
                return false;

            if (_openDropdown?.Trigger == drawable)
                CloseDropdown();

            (drawable as Button)?.ResetInteractionState();
            _regions[region].Remove(drawable);
            _items.Remove(drawable);
            _itemSizes.Remove(drawable);
            DetachWithoutDestroying(drawable);

            if (destroy)
                drawable.Destroy();

            RefreshLayout();
            return true;
        }

        public void Clear(NavigationBarRegion? region = null, bool destroy = false)
        {
            var drawables = region == null
                ? _items.Keys.ToArray()
                : _regions[region.Value].ToArray();

            foreach (var drawable in drawables)
                Remove(drawable, destroy);
        }

        public RoundedButton AddRoundedButton(NavigationBarRegion region, NavigationBarButtonOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (!string.IsNullOrEmpty(options.Text) && options.Font == null)
                throw new ArgumentException("A font is required when button text is supplied.", nameof(options));

            if (!string.IsNullOrEmpty(options.Text) && options.FontSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Font size must be greater than zero.");

            if (options.DropdownOptions?.Any(x => x != null && !string.IsNullOrEmpty(x.Text)) == true &&
                options.Font == null)
                throw new ArgumentException("A font is required when dropdown text is supplied.", nameof(options));

            var button = new RoundedButton(options.ClickAction)
            {
                Size = new ScalableVector2(options.Width, options.Height),
                WidthMode = options.WidthMode,
                HeightMode = options.HeightMode,
                AutoSizePadding = options.AutoSizePadding,
                CornerRadius = options.CornerRadius,
                PerformHoverFade = options.PerformHoverFade,
                AntiAliasedEdges = options.AntiAliasedEdges,
                ExpandLabelOnHover = options.ExpandLabelOnHover,
                AlwaysShowLabel = options.AlwaysShowLabel,
                HoverExpansionDuration = options.HoverExpansionDuration,
                ExpandedLabelRightPadding = options.ExpandedLabelRightPadding,
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

            if (options.DropdownOptions?.Count > 0)
                button.Clicked += (sender, args) => ToggleDropdown(button, options);

            return button;
        }

        public override void Destroy()
        {
            CloseDropdown();
            DisposeGeneratedGradient();
            base.Destroy();
        }

        public override void DrawToSpriteBatch()
        {
            if (Background?.Type != NavigationBarBackgroundType.Image)
            {
                base.DrawToSpriteBatch();
                return;
            }

            if (Background.Image == null || Background.Image.IsDisposed)
                return;

            if (Background.ImageFit == NavigationBarImageFit.Stretch)
            {
                base.DrawToSpriteBatch();
                return;
            }

            if (!Visible)
                return;

            switch (Background.ImageFit)
            {
                case NavigationBarImageFit.Cover:
                    DrawCoverImage();
                    break;
                case NavigationBarImageFit.Contain:
                    DrawContainImage();
                    break;
                case NavigationBarImageFit.Tile:
                    DrawTiledImage();
                    break;
                default:
                    base.DrawToSpriteBatch();
                    break;
            }
        }

        public void RefreshLayout()
        {
            if (!_initialized)
                return;

            RemoveDisposedItems();
            LayoutLeftItems();
            LayoutCenterItems();
            LayoutRightItems();
            CaptureItemSizes();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (!_initialized || _refreshingBackground)
                return;

            if (Background?.Type == NavigationBarBackgroundType.Gradient &&
                !GradientTextureMatchesRenderSize())
                RefreshBackground();

            if (Math.Abs(_layoutWidth - Width) < float.Epsilon &&
                Math.Abs(_layoutHeight - Height) < float.Epsilon)
                return;

            _layoutWidth = Width;
            _layoutHeight = Height;
            RefreshBorderLayout();
            RefreshLayout();
        }

        private void DrawCoverImage()
        {
            if (Width <= 0 || Height <= 0)
                return;

            var barAspect = Width / Height;
            var imageAspect = Image.Width / (float) Image.Height;
            Rectangle source;

            if (imageAspect > barAspect)
            {
                var sourceWidth = Math.Max(1, Math.Min(Image.Width, (int) Math.Round(Image.Height * barAspect)));
                source = new Rectangle((Image.Width - sourceWidth) / 2, 0, sourceWidth, Image.Height);
            }
            else
            {
                var sourceHeight = Math.Max(1, Math.Min(Image.Height, (int) Math.Round(Image.Width / barAspect)));
                source = new Rectangle(0, (Image.Height - sourceHeight) / 2, Image.Width, sourceHeight);
            }

            DrawImageRegion(new RectangleF(0, 0, Width, Height), source);
        }

        private void DrawContainImage()
        {
            if (Width <= 0 || Height <= 0)
                return;

            var scale = Math.Min(Width / Image.Width, Height / Image.Height);
            var imageWidth = Image.Width * scale;
            var imageHeight = Image.Height * scale;
            DrawImageRegion(new RectangleF((Width - imageWidth) / 2f, (Height - imageHeight) / 2f,
                imageWidth, imageHeight), null);
        }

        private void DrawTiledImage()
        {
            if (Width <= 0 || Height <= 0)
                return;

            for (var y = 0f; y < Height; y += Image.Height)
            {
                var tileHeight = Math.Min(Image.Height, Height - y);
                var sourceHeight = Math.Max(1, Math.Min(Image.Height, (int) Math.Ceiling(tileHeight)));

                for (var x = 0f; x < Width; x += Image.Width)
                {
                    var tileWidth = Math.Min(Image.Width, Width - x);
                    var sourceWidth = Math.Max(1, Math.Min(Image.Width, (int) Math.Ceiling(tileWidth)));
                    DrawImageRegion(new RectangleF(x, y, tileWidth, tileHeight),
                        new Rectangle(0, 0, sourceWidth, sourceHeight));
                }
            }
        }

        private void DrawImageRegion(RectangleF localBounds, Rectangle? source)
        {
            if (localBounds.Width <= 0 || localBounds.Height <= 0 || Width == 0 || Height == 0)
                return;

            var sourceWidth = source?.Width ?? Image.Width;
            var sourceHeight = source?.Height ?? Image.Height;
            var pivotPosition = new Vector2(Width * Pivot.X, Height * Pivot.Y);
            var origin = new Vector2(
                sourceWidth * (pivotPosition.X - localBounds.X) / localBounds.Width,
                sourceHeight * (pivotPosition.Y - localBounds.Y) / localBounds.Height);
            var destination = new RectangleF(RenderRectangle.Position,
                new Size2(localBounds.Width * RenderRectangle.Width / Width,
                    localBounds.Height * RenderRectangle.Height / Height));

            GameBase.Game.SpriteBatch.Draw(Image, destination, source, _color, SpriteOverallRotation, origin,
                SpriteEffect, 0f);
        }

        private bool TryCreateGradientTexture(NavigationBarGradientOptions options, out Texture2D texture)
        {
            texture = null;

            if (!IsValidGradient(options))
                return false;

            var width = GetGradientTextureWidth();
            var height = GetGradientTextureHeight();
            var pixels = new Color[width * height];
            var angle = MathHelper.ToRadians(options.AngleDegrees);
            var direction = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
            var projectionMinimum = Math.Min(0, direction.X) + Math.Min(0, direction.Y);
            var projectionMaximum = Math.Max(0, direction.X) + Math.Max(0, direction.Y);
            var projectionRange = projectionMaximum - projectionMinimum;
            var farthestCornerDistance = GetFarthestCornerDistance(options.RadialOrigin);

            for (var y = 0; y < height; y++)
            {
                var v = height == 1 ? 0.5f : y / (float) (height - 1);

                for (var x = 0; x < width; x++)
                {
                    var u = width == 1 ? 0.5f : x / (float) (width - 1);
                    float amount;

                    if (options.Type == NavigationBarGradientType.Linear)
                    {
                        amount = (Vector2.Dot(new Vector2(u, v), direction) - projectionMinimum) /
                                 projectionRange;
                    }
                    else
                    {
                        amount = Vector2.Distance(new Vector2(u, v), options.RadialOrigin) /
                                 (farthestCornerDistance * options.RadialRadius);
                    }

                    pixels[y * width + x] = SampleGradient(options.Stops, amount);
                }
            }

            texture = new Texture2D(GameBase.Game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData(pixels);
            return true;
        }

        private static bool IsValidGradient(NavigationBarGradientOptions options)
        {
            if (options == null || options.Stops == null || options.Stops.Count < 2 ||
                !Enum.IsDefined(typeof(NavigationBarGradientType), options.Type) ||
                !IsFinite(options.AngleDegrees) || !IsFinite(options.RadialOrigin.X) ||
                !IsFinite(options.RadialOrigin.Y) || !IsFinite(options.RadialRadius) ||
                options.RadialOrigin.X < 0 || options.RadialOrigin.X > 1 ||
                options.RadialOrigin.Y < 0 || options.RadialOrigin.Y > 1 || options.RadialRadius <= 0)
                return false;

            var previousPosition = float.NegativeInfinity;

            foreach (var stop in options.Stops)
            {
                if (stop == null || !IsFinite(stop.Position) || stop.Position < 0 || stop.Position > 1 ||
                    stop.Position <= previousPosition)
                    return false;

                previousPosition = stop.Position;
            }

            return true;
        }

        private static Color SampleGradient(IReadOnlyList<NavigationBarGradientStop> stops, float amount)
        {
            if (amount <= stops[0].Position)
                return stops[0].Color;

            for (var i = 1; i < stops.Count; i++)
            {
                if (amount > stops[i].Position)
                    continue;

                var previous = stops[i - 1];
                var current = stops[i];
                var interpolation = (amount - previous.Position) / (current.Position - previous.Position);
                return Color.Lerp(previous.Color, current.Color, interpolation);
            }

            return stops[stops.Count - 1].Color;
        }

        private static float GetFarthestCornerDistance(Vector2 origin)
        {
            var farthestX = Math.Max(origin.X, 1 - origin.X);
            var farthestY = Math.Max(origin.Y, 1 - origin.Y);
            return (float) Math.Sqrt(farthestX * farthestX + farthestY * farthestY);
        }

        private bool GradientTextureMatchesRenderSize() => _generatedGradientTexture != null &&
                                                           !_generatedGradientTexture.IsDisposed &&
                                                           _generatedGradientTexture.Width == GetGradientTextureWidth() &&
                                                           _generatedGradientTexture.Height == GetGradientTextureHeight();

        private int GetGradientTextureWidth() => GetGradientTextureDimension(RenderRectangle.Width);

        private int GetGradientTextureHeight() => GetGradientTextureDimension(RenderRectangle.Height);

        private static int GetGradientTextureDimension(float renderSize)
        {
            var pixels = Math.Max(1, (int) Math.Ceiling(Math.Abs(renderSize)));
            if (pixels >= GradientTextureMaximumDimension)
                return GradientTextureMaximumDimension;

            return Math.Min(GradientTextureMaximumDimension,
                (pixels + GradientTextureSizeBucket - 1) / GradientTextureSizeBucket *
                GradientTextureSizeBucket);
        }

        private void ApplyTransparentBackground()
        {
            Image = WobbleAssets.WhiteBox;
            Tint = Color.Transparent;
        }

        private void DisposeGeneratedGradient()
        {
            if (_generatedGradientTexture == null)
                return;

            _generatedGradientTexture.Dispose();
            _generatedGradientTexture = null;
        }

        private static NavigationBarBackgroundOptions CreateTransparentBackground() =>
            new NavigationBarBackgroundOptions
            {
                Type = NavigationBarBackgroundType.SolidColor,
                SolidColor = Color.Transparent
            };

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

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

        private void ToggleDropdown(RoundedButton trigger, NavigationBarButtonOptions options)
        {
            if (_openDropdown?.Trigger == trigger)
            {
                CloseDropdown();
                return;
            }

            CloseDropdown();

            var padding = Math.Max(0, options.DropdownPadding);
            var spacing = Math.Max(0, options.DropdownItemSpacing);
            var itemHeight = Math.Max(1, options.DropdownItemHeight);
            var rows = new List<RoundedButton>();
            var menu = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Image = WobbleAssets.WhiteBox,
                Tint = options.DropdownBackgroundColor ?? options.BackgroundColor
            };

            var width = trigger.Width;

            foreach (var dropdownOption in options.DropdownOptions)
            {
                if (dropdownOption == null || string.IsNullOrEmpty(dropdownOption.Text))
                    continue;

                var row = new RoundedButton
                {
                    Parent = menu,
                    Alignment = Alignment.TopLeft,
                    Height = itemHeight,
                    WidthMode = ButtonSizeMode.Auto,
                    HeightMode = ButtonSizeMode.Fixed,
                    AutoSizePadding = options.DropdownItemPadding,
                    CornerRadius = options.CornerRadius,
                    PerformHoverFade = options.PerformHoverFade,
                    AntiAliasedEdges = options.AntiAliasedEdges,
                    Tint = options.DropdownItemBackgroundColor ?? options.BackgroundColor
                };

                row.SetLabel(options.Font, dropdownOption.Text, options.FontSize,
                    options.DropdownForegroundColor ?? options.ForegroundColor);
                row.RecalculateAutoSize();
                width = Math.Max(width, row.Width);
                row.Clicked += (sender, args) =>
                {
                    dropdownOption.ClickAction?.Invoke(sender, args);
                    CloseDropdown();
                };
                rows.Add(row);
            }

            if (rows.Count == 0)
            {
                menu.Destroy();
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].WidthMode = ButtonSizeMode.Fixed;
                rows[i].Width = width;
                rows[i].X = padding;
                rows[i].Y = padding + i * (itemHeight + spacing);
            }

            menu.Size = new ScalableVector2(width + padding * 2,
                padding * 2 + rows.Count * itemHeight + (rows.Count - 1) * spacing);
            _openDropdown = new DropdownState(trigger, menu, _items[trigger]);
            PositionDropdown(_openDropdown);
        }

        private void PositionDropdown(DropdownState dropdown)
        {
            var opensDown = ScreenRectangle.Y + Height / 2f <= Window.WindowManager.Height / 2f;
            var triggerLeft = dropdown.Trigger.ScreenRectangle.X - ScreenRectangle.X;

            switch (dropdown.Region)
            {
                case NavigationBarRegion.Center:
                    dropdown.Menu.X = triggerLeft + (dropdown.Trigger.Width - dropdown.Menu.Width) / 2f;
                    break;
                case NavigationBarRegion.Right:
                    dropdown.Menu.X = triggerLeft + dropdown.Trigger.Width - dropdown.Menu.Width;
                    break;
                default:
                    dropdown.Menu.X = triggerLeft;
                    break;
            }

            dropdown.Menu.Y = opensDown ? Height : -dropdown.Menu.Height;
        }

        private void CloseDropdown()
        {
            if (_openDropdown == null)
                return;

            var menu = _openDropdown.Menu;
            _openDropdown = null;
            menu.Destroy();
        }

        private sealed class DropdownState
        {
            public RoundedButton Trigger { get; }

            public Sprite Menu { get; }

            public NavigationBarRegion Region { get; }

            public DropdownState(RoundedButton trigger, Sprite menu, NavigationBarRegion region)
            {
                Trigger = trigger;
                Menu = menu;
                Region = region;
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
                _itemSizes.Remove(drawable);
            }
        }

        private bool HaveItemSizesChanged()
        {
            foreach (var drawable in _items.Keys)
            {
                if (!_itemSizes.TryGetValue(drawable, out var size) ||
                    Math.Abs(size.X - drawable.Width) > float.Epsilon ||
                    Math.Abs(size.Y - drawable.Height) > float.Epsilon)
                    return true;
            }

            return false;
        }

        private void CaptureItemSizes()
        {
            foreach (var drawable in _items.Keys)
                _itemSizes[drawable] = new Vector2(drawable.Width, drawable.Height);
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
