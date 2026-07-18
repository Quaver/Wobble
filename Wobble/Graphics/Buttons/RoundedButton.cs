using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Wobble.Graphics.Buttons
{
    /// <summary>
    ///     Whether an axis of a <see cref="RoundedButton"/> should be a hard-coded size, or
    ///     calculated automatically from its content (<see cref="RoundedButton.Icon"/>/<see cref="RoundedButton.Label"/>) + padding.
    /// </summary>
    public enum ButtonSizeMode
    {
        Fixed,
        Auto
    }

    /// <summary>
    ///     A button with a code-generated, anti-aliased rounded-rect background instead of a pre-baked,
    ///     stretched asset. Exact-size textures are cached so multiple buttons retain normal SpriteBatch batching.
    /// </summary>
    public class RoundedButton : Button
    {
        /// <summary>
        ///     The gap between <see cref="Icon"/> and <see cref="Label"/> when both are present.
        /// </summary>
        private const int IconLabelSpacing = 8;

        /// <summary>
        /// </summary>
        public ButtonSizeMode WidthMode { get; set; } = ButtonSizeMode.Fixed;

        /// <summary>
        /// </summary>
        public ButtonSizeMode HeightMode { get; set; } = ButtonSizeMode.Fixed;

        /// <summary>
        ///     Total padding added around the content bounding box when <see cref="WidthMode"/>/<see cref="HeightMode"/> is <see cref="ButtonSizeMode.Auto"/>.
        /// </summary>
        public Vector2 AutoSizePadding { get; set; } = new Vector2(32, 12);

        private float? _cornerRadius;

        /// <summary>
        ///     The corner radius, in pixels. Defaults to <c>null</c>, which resolves to a full pill (<see cref="Drawable.Height"/> / 2).
        /// </summary>
        public float? CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                UpdateBackgroundTexture();
            }
        }

        /// <summary>
        ///     Whether to dim the button's <see cref="Sprite.Alpha"/> on hover, matching the existing <c>IconButton</c> feedback.
        ///     Set to <c>false</c> for buttons that implement their own hover/selected visuals.
        /// </summary>
        public bool PerformHoverFade { get; set; } = true;

        /// <summary>
        ///     Whether the generated rounded edge uses a one-pixel anti-aliased transition.
        /// </summary>
        public bool AntiAliasedEdges
        {
            get => _antiAliasedEdges;
            set
            {
                if (_antiAliasedEdges == value)
                    return;

                _antiAliasedEdges = value;
                UpdateBackgroundTexture();
            }
        }

        private bool _antiAliasedEdges = true;

        /// <summary>
        ///     Whether an icon button should expand to reveal its label while hovered.
        /// </summary>
        public bool ExpandLabelOnHover { get; set; }

        /// <summary>
        ///     Whether the label should remain expanded when the button is not hovered, such as for an active navigation item.
        ///     This can be changed at runtime.
        /// </summary>
        public bool AlwaysShowLabel
        {
            get => _alwaysShowLabel;
            set
            {
                if (_alwaysShowLabel == value)
                    return;

                _alwaysShowLabel = value;

                if (value && Icon != null && Label != null)
                    LayoutContent();
            }
        }

        private bool _alwaysShowLabel;

        /// <summary>
        ///     The duration, in milliseconds, of the hover label expansion and collapse.
        /// </summary>
        public int HoverExpansionDuration { get; set; } = 150;

        /// <summary>
        ///     Additional right-side padding for an expanded label, useful for compensating font visual bounds.
        /// </summary>
        public float ExpandedLabelRightPadding { get; set; }

        public Sprite Icon { get; private set; }

        public SpriteTextPlus Label { get; private set; }

        private float CollapsedWidth { get; set; }

        private float ExpandedWidth { get; set; }

        private float HoverExpansionProgress { get; set; }

        private bool HoverExpansionInitialized { get; set; }

        private Vector2 LastContentSize { get; set; } = new Vector2(float.NaN, float.NaN);

        private SpriteBatchOptions HoverClipSpriteBatchOptions { get; } = RoundedRectShader.CreateScissorSafeOptions();

        /// <inheritdoc />
        public RoundedButton(EventHandler clickAction = null) : base(clickAction) => SetChildrenAlpha = true;

        /// <summary>
        ///     Creates/updates the icon child, laying content back out afterwards.
        /// </summary>
        public void SetIcon(Texture2D texture, Vector2? size = null)
        {
            var iconSize = size ?? new Vector2(16, 16);

            if (Icon == null)
            {
                Icon = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter,
                    UsePreviousSpriteBatchOptions = true
                };
            }

            Icon.Image = texture;
            Icon.Size = new ScalableVector2(iconSize.X, iconSize.Y);

            LayoutContent();
        }

        /// <summary>
        ///     Creates/updates the label child, laying content back out afterwards.
        /// </summary>
        public void SetLabel(WobbleFontStore font, string text, int fontSize, Color? color = null)
        {
            if (Label == null)
            {
                Label = new SpriteTextPlus(font, text, fontSize)
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter,
                    UsePreviousSpriteBatchOptions = true
                };
            }
            else
                Label.Text = text;

            if (color != null)
                Label.Tint = color.Value;

            Label.Y = 0;
            LayoutContent();
        }

        /// <summary>
        ///     Positions <see cref="Icon"/>/<see cref="Label"/> relative to each other, then recalculates
        ///     any axis in <see cref="ButtonSizeMode.Auto"/> mode from the resulting content bounds.
        /// </summary>
        private void LayoutContent()
        {
            if (Icon != null && Label != null)
            {
                if (ExpandLabelOnHover || AlwaysShowLabel)
                {
                    if (!HoverExpansionInitialized)
                    {
                        CollapsedWidth = Width;
                        HoverExpansionInitialized = true;
                    }

                    ExpandedWidth = Math.Max(CollapsedWidth,
                        Icon.Width + IconLabelSpacing + Label.Width + AutoSizePadding.X +
                        ExpandedLabelRightPadding);
                    ApplyHoverExpansion();

                    return;
                }

                var totalWidth = Icon.Width + IconLabelSpacing + Label.Width;
                Icon.X = -totalWidth / 2f + Icon.Width / 2f;
                Label.X = Icon.X + Icon.Width / 2f + IconLabelSpacing + Label.Width / 2f;
            }

            RecalculateAutoSize();
        }

        /// <summary>
        ///     Recalculates <see cref="Drawable.Width"/>/<see cref="Drawable.Height"/> for whichever axes
        ///     are in <see cref="ButtonSizeMode.Auto"/> mode, based on the current <see cref="Icon"/>/<see cref="Label"/> content.
        /// </summary>
        public void RecalculateAutoSize()
        {
            if (WidthMode == ButtonSizeMode.Auto && !ExpandLabelOnHover && !AlwaysShowLabel)
            {
                var contentWidth = (Icon?.Width ?? 0) + (Label?.Width ?? 0);

                if (Icon != null && Label != null)
                    contentWidth += IconLabelSpacing;

                Width = contentWidth + AutoSizePadding.X;
            }

            if (HeightMode == ButtonSizeMode.Auto)
                Height = Math.Max(Icon?.Height ?? 0, Label?.Height ?? 0) + AutoSizePadding.Y;
        }

        /// <inheritdoc />
        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            UpdateBackgroundTexture();
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            var isTransitioning = HoverExpansionInitialized && HoverExpansionProgress > 0 &&
                                  HoverExpansionProgress < 1;

            if (!isTransitioning)
            {
                base.Draw(gameTime);
                return;
            }

            var graphicsDevice = GameBase.Game.GraphicsDevice;
            var previousScissorRectangle = graphicsDevice.ScissorRectangle;
            var widthScale = GameBase.Game.Graphics.PreferredBackBufferWidth / WindowManager.Width;
            var heightScale = GameBase.Game.Graphics.PreferredBackBufferHeight / WindowManager.Height;
            var buttonRectangle = new Rectangle(
                (int) (ScreenRectangle.X * widthScale),
                (int) (ScreenRectangle.Y * heightScale),
                (int) Math.Ceiling(ScreenRectangle.Width * widthScale),
                (int) Math.Ceiling(ScreenRectangle.Height * heightScale));

            graphicsDevice.ScissorRectangle = Rectangle.Intersect(previousScissorRectangle, buttonRectangle);
            SpriteBatchOptions = HoverClipSpriteBatchOptions;
            base.Draw(gameTime);
            _ = GameBase.Game.TryEndBatch();
            SpriteBatchOptions = null;
            graphicsDevice.ScissorRectangle = previousScissorRectangle;
        }

        private void UpdateBackgroundTexture()
        {
            if (Width <= 0 || Height <= 0)
                return;

            var radius = Math.Min(CornerRadius ?? Height / 2f, Math.Min(Width, Height) / 2f);
            var texture = RoundedRectTextureCache.Get(Width, Height, radius, AntiAliasedEdges);

            if (Image != texture)
                Image = texture;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (PerformHoverFade &&
                !Animations.Exists(animation => animation.Properties == AnimationProperty.Alpha))
            {
                var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
                var targetAlpha = IsHovered ? 0.75f : 1f;

                if (Alpha != targetAlpha)
                {
                    var alpha = AnimationMath.Damp(Alpha, targetAlpha, dt, 60);
                    Alpha = Math.Abs(alpha - targetAlpha) < 0.001f ? targetAlpha : alpha;
                }
            }

            var contentSize = new Vector2(
                (Icon?.Width ?? 0) + (Label?.Width ?? 0),
                Math.Max(Icon?.Height ?? 0, Label?.Height ?? 0));

            if (contentSize != LastContentSize)
            {
                LastContentSize = contentSize;
                LayoutContent();
            }

            if (HoverExpansionInitialized && Icon != null && Label != null)
                UpdateHoverExpansion(gameTime);
        }

        private void UpdateHoverExpansion(GameTime gameTime)
        {
            var target = IsHovered || AlwaysShowLabel ? 1f : 0f;

            if (Math.Abs(target - HoverExpansionProgress) < float.Epsilon)
                return;

            var duration = target > HoverExpansionProgress
                ? Math.Max(1, HoverExpansionDuration)
                : Math.Max(1, HoverExpansionDuration * 0.65f);
            var change = (float) (gameTime.ElapsedGameTime.TotalMilliseconds / duration);

            HoverExpansionProgress = target > HoverExpansionProgress
                ? Math.Min(target, HoverExpansionProgress + change)
                : Math.Max(target, HoverExpansionProgress - change);

            ApplyHoverExpansion();
        }

        private void ApplyHoverExpansion()
        {
            var progress = MathHelper.SmoothStep(0, 1, HoverExpansionProgress);
            var totalWidth = Icon.Width + IconLabelSpacing + Label.Width;
            var expandedContentOffset = -ExpandedLabelRightPadding / 2f;
            var expandedIconX = -totalWidth / 2f + Icon.Width / 2f + expandedContentOffset;
            var expandedLabelX = expandedIconX + Icon.Width / 2f + IconLabelSpacing + Label.Width / 2f;
            var collapsedLabelX = Icon.Width / 2f + IconLabelSpacing + Label.Width / 2f;

            Width = MathHelper.Lerp(CollapsedWidth, ExpandedWidth, progress);
            Icon.X = MathHelper.Lerp(0, expandedIconX, progress);
            Label.X = MathHelper.Lerp(collapsedLabelX, expandedLabelX, progress);
            Label.Alpha = MathHelper.Clamp(progress * 3, 0, 1);
            Label.Visible = HoverExpansionProgress > 0;
        }
    }
}
