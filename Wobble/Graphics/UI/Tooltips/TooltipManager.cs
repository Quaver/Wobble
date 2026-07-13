using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Wobble.Graphics.UI.Tooltips
{
    public static class TooltipManager
    {
        private sealed class Binding : IDisposable
        {
            public Drawable Target { get; }
            public TooltipOptions Options { get; }
            public double HoveredMilliseconds { get; set; }
            private volatile bool disposed;
            public bool Disposed => disposed;

            public Binding(Drawable target, TooltipOptions options)
            {
                Target = target;
                Options = options;
            }

            public void Dispose()
            {
                disposed = true;
                if (ReferenceEquals(_active, this))
                    Hide();
            }
        }

        private struct Candidate
        {
            public TooltipAnchor Anchor;
            public RectangleF Rectangle;
            public float TextWidth;
            public float VisibleArea;
            public bool Fits;
        }

        private static readonly List<Binding> Bindings = new List<Binding>();
        private static readonly List<Binding> BindingSnapshot = new List<Binding>(64);
        private static readonly TooltipAnchor[] Anchors = (TooltipAnchor[]) Enum.GetValues(typeof(TooltipAnchor));
        private static Binding _active;
        private static Container _overlay;
        private static Sprite _background;
        private static Sprite _border;
        private static SpriteTextPlus _text;
        private static WobbleFontStore _font;
        private static int _fontWeight = int.MinValue;
        private static int _textSize;
        private static string _displayedText;
        private static Binding _layoutBinding;
        private static RectangleF _layoutTarget;
        private static float _layoutViewportWidth = -1;
        private static float _layoutViewportHeight = -1;
        private static TooltipAnchor _layoutAnchor;
        private static float _layoutMaximumWidth;
        private static float _layoutPadding;
        private static float _layoutOffset;
        private static float _layoutBorderThickness;
        private static bool _layoutRoundedCorners;
        private static float? _layoutCornerRadius;
        private static WobbleFontStore _layoutFont;
        private static int _layoutTextSize;
        private static string _layoutText;

        public static TooltipTheme Theme { get; set; } = new TooltipTheme();

        /// <summary>
        ///     Optional host-provided filter for suppressing targets occluded by global overlays.
        /// </summary>
        public static Func<Drawable, bool> TargetEligibilityFilter { get; set; }

        public static IDisposable Attach(Drawable target, TooltipOptions options)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var binding = new Binding(target, options);
            lock (Bindings)
                Bindings.Add(binding);

            return binding;
        }

        public static void Update(GameTime gameTime)
        {
            lock (Bindings)
            {
                for (var i = Bindings.Count - 1; i >= 0; i--)
                {
                    var binding = Bindings[i];
                    if (binding.Disposed || binding.Target == null || binding.Target.IsDisposed)
                        Bindings.RemoveAt(i);
                }

                BindingSnapshot.Clear();
                BindingSnapshot.AddRange(Bindings);
            }

            Binding hovered = null;
            for (var i = 0; i < BindingSnapshot.Count; i++)
            {
                var binding = BindingSnapshot[i];
                if (binding.Disposed || !IsEligible(binding.Target) ||
                    !GraphicsHelper.RectangleContains(binding.Target.ScreenMinimumBoundingRectangle,
                        MouseManager.CurrentState.Position))
                    continue;

                if (hovered == null || binding.Target.DrawOrder > hovered.Target.DrawOrder)
                    hovered = binding;
            }

            for (var i = 0; i < BindingSnapshot.Count; i++)
            {
                var binding = BindingSnapshot[i];
                if (ReferenceEquals(binding, hovered))
                    binding.HoveredMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;
                else
                    binding.HoveredMilliseconds = 0;
            }

            if (hovered == null || hovered.HoveredMilliseconds <
                (hovered.Options.HoverDelayMilliseconds ?? Theme.HoverDelayMilliseconds))
            {
                Hide();
                return;
            }

            _active = hovered;
            EnsureVisuals(hovered);
            Position(hovered);
            _overlay.Update(gameTime);
        }

        public static void Draw(GameTime gameTime)
        {
            if (_active != null && _overlay != null)
                _overlay.Draw(gameTime);
        }

        private static bool IsEligible(Drawable target)
        {
            if (target.IsDisposed || !target.Visible || target.ScreenMinimumBoundingRectangle.Width <= 0 ||
                target.ScreenMinimumBoundingRectangle.Height <= 0 || !DialogManager.IsInputAllowed(target))
                return false;

            for (var parent = target.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.IsDisposed || !parent.Visible)
                    return false;

                if (parent is ScrollContainer scrollContainer &&
                    !GraphicsHelper.RectangleContains(scrollContainer.ScreenRectangle,
                        MouseManager.CurrentState.Position))
                {
                    return false;
                }
            }

            return TargetEligibilityFilter?.Invoke(target) ?? true;
        }

        private static void EnsureVisuals(Binding binding)
        {
            var style = binding.Options.Style;
            var weight = style?.TextWeight ?? Theme.TextWeight;
            var font = _font != null && weight == _fontWeight ? _font : ResolveFont(weight);
            _fontWeight = weight;
            var size = style?.TextSize ?? Theme.TextSize;

            if (_overlay == null)
            {
                _overlay = new Container { DrawIfOffScreen = true };
                // RoundedButton uses the same exact-size rounded texture cache. Keep the border behind a
                // slightly inset background so both edges share the same anti-aliased rounding path.
                _border = new Sprite
                {
                    Parent = _overlay,
                    DrawIfOffScreen = true
                };
                _background = new Sprite
                {
                    Parent = _overlay,
                    DrawIfOffScreen = true
                };
            }

            if (_text == null || !ReferenceEquals(font, _font) || size != _textSize)
            {
                _text?.Destroy();
                _font = font;
                _textSize = size;
                _text = new SpriteTextPlus(font, binding.Options.Text ?? "", size)
                {
                    Parent = _overlay,
                    DrawIfOffScreen = true
                };
                _displayedText = binding.Options.Text ?? "";
            }
            else if (_displayedText != (binding.Options.Text ?? ""))
            {
                _displayedText = binding.Options.Text ?? "";
                _text.Text = _displayedText;
            }

            _background.Tint = style?.BackgroundColor ?? Theme.BackgroundColor;
            _text.Tint = style?.TextColor ?? Theme.TextColor;
            _border.Tint = style?.BorderColor ?? Theme.BorderColor;
            _border.Visible = Math.Max(0, style?.BorderThickness ?? Theme.BorderThickness) > 0;
        }

        private static WobbleFontStore ResolveFont(int weight)
        {
            var fonts = Theme.Fonts;
            if (fonts != null && fonts.Count > 0)
                return fonts.OrderBy(x => Math.Abs(x.Key - weight)).ThenBy(x => x.Key).First().Value;

            var fallback = FontManager.WobbleFonts.Values.FirstOrDefault();
            if (fallback == null)
                throw new InvalidOperationException("A tooltip requires at least one font in TooltipManager.Theme.Fonts or FontManager.");
            return fallback;
        }

        private static void Position(Binding binding)
        {
            var options = binding.Options;
            var padding = Math.Max(0, options.Padding ?? Theme.Padding);
            var offset = Math.Max(0, options.Offset ?? Theme.Offset);
            var border = _border.Visible
                ? Math.Max(0, options.Style?.BorderThickness ?? Theme.BorderThickness)
                : 0;
            var inset = padding + border;
            var viewport = new RectangleF(0, 0, WindowManager.Width, WindowManager.Height);
            var requestedMaxWidth = Math.Max(1, options.MaximumWidth ?? Theme.MaximumWidth);
            var target = binding.Target.ScreenMinimumBoundingRectangle;
            var rounded = options.Style?.RoundedCorners ?? Theme.RoundedCorners;
            var cornerRadius = options.Style?.CornerRadius ?? Theme.CornerRadius;

            if (ReferenceEquals(binding, _layoutBinding) && RectanglesEqual(target, _layoutTarget) &&
                WindowManager.Width == _layoutViewportWidth && WindowManager.Height == _layoutViewportHeight &&
                options.Anchor == _layoutAnchor && requestedMaxWidth == _layoutMaximumWidth &&
                padding == _layoutPadding && offset == _layoutOffset && border == _layoutBorderThickness &&
                rounded == _layoutRoundedCorners && cornerRadius == _layoutCornerRadius &&
                ReferenceEquals(_font, _layoutFont) && _textSize == _layoutTextSize &&
                _displayedText == _layoutText)
            {
                return;
            }

            var candidates = new List<Candidate>(Anchors.Length);

            foreach (var anchor in Anchors)
            {
                var availableWidth = AvailableTextWidth(anchor, target, viewport, offset, inset);
                var textWidth = Math.Max(1, Math.Min(requestedMaxWidth, availableWidth));
                _text.MaxWidth = textWidth;
                var width = _text.Width + inset * 2;
                var height = _text.Height + inset * 2;
                var rectangle = Place(anchor, target, width, height, offset);
                var intersection = RectangleF.Intersection(rectangle, viewport);
                candidates.Add(new Candidate
                {
                    Anchor = anchor,
                    Rectangle = rectangle,
                    TextWidth = textWidth,
                    VisibleArea = Math.Max(0, intersection.Width) * Math.Max(0, intersection.Height),
                    Fits = rectangle.Left >= viewport.Left && rectangle.Top >= viewport.Top &&
                           rectangle.Right <= viewport.Right && rectangle.Bottom <= viewport.Bottom
                });
            }

            var selected = candidates.Where(x => x.Fits)
                .OrderBy(x => AnchorDistance(options.Anchor, x.Anchor))
                .ThenByDescending(x => x.TextWidth)
                .FirstOrDefault();

            if (!candidates.Any(x => x.Fits))
                selected = candidates.OrderByDescending(x => x.VisibleArea)
                    .ThenBy(x => AnchorDistance(options.Anchor, x.Anchor))
                    .ThenByDescending(x => x.TextWidth)
                    .First();

            _text.MaxWidth = selected.TextWidth;
            var finalWidth = Math.Min(viewport.Width, _text.Width + inset * 2);
            var finalHeight = Math.Min(viewport.Height, _text.Height + inset * 2);
            var final = Place(selected.Anchor, target, finalWidth, finalHeight, offset);
            final.X = MathHelper.Clamp(final.X, viewport.Left, Math.Max(viewport.Left, viewport.Right - final.Width));
            final.Y = MathHelper.Clamp(final.Y, viewport.Top, Math.Max(viewport.Top, viewport.Bottom - final.Height));

            _background.Position = new ScalableVector2(final.X, final.Y);
            _background.Size = new ScalableVector2(final.Width, final.Height);
            _border.Position = _background.Position;
            _border.Size = _background.Size;
            _text.Position = new ScalableVector2(final.X + inset, final.Y + inset);

            UpdateBackgroundTextures(options, final, border);

            _layoutBinding = binding;
            _layoutTarget = target;
            _layoutViewportWidth = WindowManager.Width;
            _layoutViewportHeight = WindowManager.Height;
            _layoutAnchor = options.Anchor;
            _layoutMaximumWidth = requestedMaxWidth;
            _layoutPadding = padding;
            _layoutOffset = offset;
            _layoutBorderThickness = border;
            _layoutRoundedCorners = rounded;
            _layoutCornerRadius = cornerRadius;
            _layoutFont = _font;
            _layoutTextSize = _textSize;
            _layoutText = _displayedText;
        }

        private static bool RectanglesEqual(RectangleF first, RectangleF second) =>
            first.X == second.X && first.Y == second.Y && first.Width == second.Width &&
            first.Height == second.Height;

        private static void UpdateBackgroundTextures(TooltipOptions options, RectangleF rectangle,
            float borderThickness)
        {
            var rounded = options.Style?.RoundedCorners ?? Theme.RoundedCorners;
            var configuredRadius = options.Style?.CornerRadius ?? Theme.CornerRadius;
            var outerRadius = rounded
                ? Math.Min(configuredRadius ?? rectangle.Height / 2f,
                    Math.Min(rectangle.Width, rectangle.Height) / 2f)
                : 0;

            _border.Image = RoundedRectTextureCache.Get(rectangle.Width, rectangle.Height, outerRadius);

            if (borderThickness <= 0)
            {
                _background.Position = new ScalableVector2(rectangle.X, rectangle.Y);
                _background.Size = new ScalableVector2(rectangle.Width, rectangle.Height);
                _background.Image = _border.Image;
                return;
            }

            var innerWidth = Math.Max(1, rectangle.Width - borderThickness * 2);
            var innerHeight = Math.Max(1, rectangle.Height - borderThickness * 2);
            var innerRadius = Math.Max(0, outerRadius - borderThickness);
            _background.Position = new ScalableVector2(rectangle.X + borderThickness,
                rectangle.Y + borderThickness);
            _background.Size = new ScalableVector2(innerWidth, innerHeight);
            _background.Image = RoundedRectTextureCache.Get(innerWidth, innerHeight, innerRadius);
        }

        private static float AvailableTextWidth(TooltipAnchor anchor, RectangleF target, RectangleF viewport,
            float offset, float inset)
        {
            float available;
            if (anchor == TooltipAnchor.CenterLeft)
                available = target.Left - viewport.Left - offset;
            else if (anchor == TooltipAnchor.CenterRight)
                available = viewport.Right - target.Right - offset;
            else
                available = viewport.Width;

            return Math.Max(1, available - inset * 2);
        }

        private static RectangleF Place(TooltipAnchor anchor, RectangleF target, float width, float height,
            float offset)
        {
            switch (anchor)
            {
                case TooltipAnchor.TopLeft:
                    return new RectangleF(target.Left, target.Top - offset - height, width, height);
                case TooltipAnchor.TopCenter:
                    return new RectangleF(target.Center.X - width / 2, target.Top - offset - height, width, height);
                case TooltipAnchor.TopRight:
                    return new RectangleF(target.Right - width, target.Top - offset - height, width, height);
                case TooltipAnchor.CenterRight:
                    return new RectangleF(target.Right + offset, target.Center.Y - height / 2, width, height);
                case TooltipAnchor.BottomRight:
                    return new RectangleF(target.Right - width, target.Bottom + offset, width, height);
                case TooltipAnchor.BottomCenter:
                    return new RectangleF(target.Center.X - width / 2, target.Bottom + offset, width, height);
                case TooltipAnchor.BottomLeft:
                    return new RectangleF(target.Left, target.Bottom + offset, width, height);
                case TooltipAnchor.CenterLeft:
                    return new RectangleF(target.Left - offset - width, target.Center.Y - height / 2, width, height);
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
            }
        }

        private static int AnchorDistance(TooltipAnchor from, TooltipAnchor to)
        {
            var delta = Math.Abs((int) from - (int) to);
            return Math.Min(delta, Anchors.Length - delta);
        }

        private static void Hide()
        {
            if (_active == null && _overlay == null)
                return;

            _active = null;

            // SpriteTextPlus caches its lines through scheduled render-target draws. Keeping the
            // invisible overlay alive would therefore retain and potentially refresh hidden text.
            // Destroying it also makes already queued cache callbacks exit through IsDisposed.
            _overlay?.Destroy();
            _overlay = null;
            _background = null;
            _border = null;
            _text = null;
            _font = null;
            _fontWeight = int.MinValue;
            _textSize = 0;
            _displayedText = null;
            _layoutBinding = null;
            _layoutViewportWidth = -1;
            _layoutViewportHeight = -1;
            _layoutFont = null;
            _layoutText = null;
        }
    }
}
