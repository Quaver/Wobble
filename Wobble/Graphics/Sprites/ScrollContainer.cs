using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Input;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    public class ScrollContainer : Sprite
    {
        private static List<ScrollContainer> MouseWheelInputCapturers { get; } =
            new List<ScrollContainer>();

        private bool _capturesMouseWheelInput;

        private Dictionary<Drawable, ContainedDrawableWidthOptions> ContainedDrawableWidths { get; } = new Dictionary<Drawable, ContainedDrawableWidthOptions>();

        private float PreviousScrollbarTextureWidth { get; set; } = float.NaN;
        private float PreviousScrollbarTextureHeight { get; set; } = float.NaN;
        private float PreviousScrollbarTrackTextureHeight { get; set; } = float.NaN;
        private float PreviousScrollbarCornerRadius { get; set; }

        /// <summary>
        ///     The fixed viewport that clips scrolling content before the scrollbar lane.
        /// </summary>
        public HorizontalClippingContainer ContentViewport { get; }

        /// <summary>
        ///     The content that holds and is a parent of all sprites
        /// </summary>
        public Container ContentContainer { get; }

        /// <summary>
        ///     Y-position of the content container
        /// </summary>
        public float CurrentY { get => ContentContainer.Y; }

        /// <summary>
        ///     Y-position of the content container in the previous frame
        /// </summary>
        public float PreviousY { get; private set; }

        /// <summary>
        ///     The scroll bar
        /// </summary>
        public Sprite Scrollbar { get; }

        /// <summary>
        ///     The background displayed behind the scroll bar
        /// </summary>
        public Sprite ScrollbarBackground { get; }

        /// <summary>
        ///     Whether the scroll bar background is displayed while the content can be scrolled
        /// </summary>
        public bool ShowScrollbarBackground { get; set; }

        /// <summary>
        ///     Whether the scrollbar width is reserved outside of the content viewport.
        /// </summary>
        public bool ReserveScrollbarSpace { get; set; }

        /// <summary>
        ///     Additional spacing between the content viewport and the scrollbar.
        /// </summary>
        public float ScrollbarContentGap { get; set; }

        /// <summary>
        ///     Spacing between the scrollbar and the right edge of the scroll container.
        /// </summary>
        public float ScrollbarRightGap { get; set; }

        /// <summary>
        ///     Whether the scrollbar is currently rendered for overflowing content.
        /// </summary>
        public bool IsScrollbarDisplayed => Scrollbar.Visible && ContentContainer.Height > Height;

        /// <summary>
        ///     The horizontal space available to scrolling content.
        /// </summary>
        public float ContentViewportWidth => Math.Max(1, Width -
            (ReserveScrollbarSpace && IsScrollbarDisplayed ? Scrollbar.Width + ScrollbarContentGap + ScrollbarRightGap : 0));

        /// <summary>
        ///     The corner radius used by both the scroll bar and its track. A value of zero keeps square corners.
        /// </summary>
        public float ScrollbarCornerRadius { get; set; }

        /// <summary>
        ///     If the container allows scrolling by dragging the scrollbar
        /// </summary>
        public bool AllowScrollbarDragging { get; set; } = false;

        /// <summary>
        ///     Keeps track of whether the user is dragging to allow for continued dragging outside normal bounds
        /// </summary>
        public bool IsScrollbarDragging { get; private set; } = false;

        /// <summary>
        ///     Used to align scrollbar movement with mouse cursor when scrollbar is being dragged
        /// </summary>
        public float ScrollbarDraggingOffset { get; private set; }

        /// <summary>
        ///     If the container allows fast scrolling with the middle mouse button
        /// </summary>
        public bool AllowMiddleMouseDragging { get; set; } = true;

        /// <summary>
        ///     Keeps track of whether the user is dragging to allow for continued dragging outside normal bounds
        /// </summary>
        public bool IsMiddleMouseDragging { get; private set; } = false;

        /// <summary>
        ///     The target y position of the container.
        /// </summary>
        public float TargetY { get; set; }

        /// <summary>
        ///     The target y position in the previous frame.
        /// </summary>
        public float PreviousTargetY { get; set; }

        /// <summary>
        ///     The speed at which the container scrolls.
        /// </summary>
        public int ScrollSpeed { get; set; } = 50;

        /// <summary>
        ///      The easing type when scrolling.
        /// </summary>
        public Easing EasingType { get; set; } = Easing.Linear;

        /// <summary>
        ///     The time to complete the scroll.
        /// </summary>
        public int TimeToCompleteScroll { get; set; } = 75;

        /// <summary>
        ///     The time to complete the scroll when the scroll bar is dragged
        /// </summary>
        public int TimeToCompleteScrollbarDragScroll { get; set; } = 600;

        /// <summary>
        ///     The time to complete the scroll when the middle mouse button is pressed
        /// </summary>
        public int TimeToCompleteMiddleMouseScroll { get; set; } = 600;

        /// <summary>
        ///     Determines if the scrolling input is enabled for the container.
        /// </summary>
        public bool InputEnabled { get; set; }

        /// <summary>
        ///     Prevents other scroll containers from handling mouse wheel input while this
        ///     container's capture is active. This is useful for nested and overlay scroll areas.
        /// </summary>
        public bool CapturesMouseWheelInput
        {
            get => _capturesMouseWheelInput;
            set
            {
                if (_capturesMouseWheelInput == value)
                    return;

                _capturesMouseWheelInput = value;
                if (value)
                    MouseWheelInputCapturers.Add(this);
                else
                    MouseWheelInputCapturers.Remove(this);
            }
        }

        /// <summary>
        ///     The minimum y the scrollbar will be clamped to
        /// </summary>
        protected int MinScrollBarY { get; set; }

        /// <summary>
        ///     If <see cref="MinScrollBarY"/> will be taken into account
        /// </summary>
        public bool IsMinScrollYEnabled { get; set; } = false;


        public Bindable<bool> InvertedScrollingOverride { get; set; }

        /// <summary>
        /// Global: null, true or false
        /// Override: null, true or false
        /// Local behavior: override if it's set; follows global if not set; false if global is null and local not set;
        /// </summary>
        public bool InvertedScrolling => InvertedScrollingOverride?.Value ?? GlobalInvertedScrolling?.Value ?? false;

        /// <summary>
        /// The global bindable toggle for inverted scrolling
        /// </summary>
        public static Bindable<bool> GlobalInvertedScrolling { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScrollContainer(ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false)
        {
            Size = size;

            // Create the SpriteBatchOptions with scissor rect enabled.
            SpriteBatchOptions = new SpriteBatchOptions
            {
                SortMode = SpriteSortMode.Deferred,
                BlendState = BlendState.NonPremultiplied,
                RasterizerState = new RasterizerState
                {
                    ScissorTestEnable = true,
                },
            };

            ContentViewport = new HorizontalClippingContainer
            {
                Parent = this,
                Size = size
            };

            // Create container in which all scrolling contents will be children of.
            ContentContainer = new Container(contentSize, new ScalableVector2(0, 0))
            {
                Parent = ContentViewport,
                UsePreviousSpriteBatchOptions = true
            };

            // Choose starting location of the scroll container
            ContentContainer.Y = startFromBottom ? -ContentContainer.Height : 0;

            ScrollbarBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Width = 15,
                Height = Height,
                Tint = Color.Transparent,
                X = 0,
                Visible = false
            };

            // Create the scroll bar.
            Scrollbar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Width = 15,
                Tint = Color.Black,
                X = 0
            };

            TargetY = ContentContainer.Y;
            PreviousY = ContentContainer.Y;
            PreviousTargetY = TargetY;
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            RefreshContentLayout();

            var contentFits = ContentContainer.Height <= Height;

            ScrollbarBackground.Width = Scrollbar.Width;
            ScrollbarBackground.Height = Height;
            Scrollbar.X = ReserveScrollbarSpace && IsScrollbarDisplayed ? -ScrollbarRightGap : 0;
            ScrollbarBackground.X = Scrollbar.X;
            ScrollbarBackground.Visible = ShowScrollbarBackground && Scrollbar.Visible && !contentFits;

            // Content that fits the viewport cannot be scrolled, so it should not produce scrollbar geometry.
            Scrollbar.Height = contentFits ? 0 : Height / ContentContainer.Height * Height;

            // Set min scroll height to 30.
            if (!contentFits && Scrollbar.Height < 30)
                Scrollbar.Height = 30;

            if (!contentFits)
                UpdateScrollbarTextures();

            // Scrollbar Dragging
            if (AllowScrollbarDragging && !IsMiddleMouseDragging)
            {
                if (InputEnabled && !IsScrollbarDragging && MouseManager.IsUniquePress(MouseButton.Left) && Scrollbar.IsHovered())
                {
                    IsScrollbarDragging = true;
                    ScrollbarDraggingOffset = Scrollbar.ScreenRectangle.Y - MouseManager.CurrentState.Position.Y;
                }
                else
                {
                    IsScrollbarDragging = IsScrollbarDragging && MouseManager.CurrentState.LeftButton == ButtonState.Pressed;
                }

                if (IsScrollbarDragging)
                {
                    var percent = (MouseManager.CurrentState.Position.Y + ScrollbarDraggingOffset - Scrollbar.Parent.ScreenRectangle.Y) / Scrollbar.Parent.Height;
                    TargetY = -ContentContainer.Height * percent;
                }
            }

            // Middle mouse button dragging
            if (AllowMiddleMouseDragging && !IsScrollbarDragging)
            {
                IsMiddleMouseDragging = MouseManager.CurrentState.MiddleButton == ButtonState.Pressed &&
                                        ((IsHovered() && InputEnabled) || IsMiddleMouseDragging);

                if (IsMiddleMouseDragging)
                {
                    var percent = MathHelper.Clamp((MouseManager.CurrentState.Y - ScreenRectangle.Y) / ScreenRectangle.Height, 0, 1);
                    TargetY = -ContentContainer.Height * percent;
                }
            }

            // Scroll wheel scrolling
            if (InputEnabled && !IsScrollbarDragging && !IsMiddleMouseDragging)
            {
                var mouseWheelInputCaptured = IsMouseWheelInputCapturedByAnotherContainer();
                if (!mouseWheelInputCaptured && MouseManager.IsScrollingUp(InvertedScrolling))
                    TargetY += ScrollSpeed;
                else if (!mouseWheelInputCaptured && MouseManager.IsScrollingDown(InvertedScrolling))
                    TargetY -= ScrollSpeed;
                else if (KeyboardManager.IsUniqueKeyPress(Keys.PageUp))
                    TargetY += ScrollSpeed * 5;
                else if (KeyboardManager.IsUniqueKeyPress(Keys.PageDown))
                    TargetY -= ScrollSpeed * 5;
            }

            // Make sure content container is clamped to the viewport.
            TargetY = contentFits ? 0 : MathHelper.Clamp(TargetY, -ContentContainer.Height + Height, 0);

            // Calculate the scrollbar's y position.
            if (contentFits)
                Scrollbar.Y = 0;
            else
            {
                var percentage = Math.Abs(-ContentContainer.Y / (-ContentContainer.Height + Height) * 100);
                Scrollbar.Y = percentage / 100 * (Scrollbar.Parent.Height - Scrollbar.Height) -
                              (Scrollbar.Parent.Height - Scrollbar.Height);
            }

            if (IsMinScrollYEnabled && Scrollbar.Y < MinScrollBarY)
                Scrollbar.Y = MinScrollBarY;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (TargetY != PreviousTargetY)
            {
                ContentContainer.Animations.Clear();

                var timeToComplete = IsScrollbarDragging ? TimeToCompleteScrollbarDragScroll :
                                     IsMiddleMouseDragging ? TimeToCompleteMiddleMouseScroll :
                                     TimeToCompleteScroll;

                ContentContainer.Animations.Add(new Animation(AnimationProperty.Y, EasingType,
                                                            ContentContainer.Y, TargetY, timeToComplete));
            }

            PreviousTargetY = TargetY;
            PreviousY = ContentContainer.Y;
            base.Update(gameTime);
        }

        private void UpdateScrollbarTextures()
        {
            if (ScrollbarCornerRadius <= 0)
            {
                if (PreviousScrollbarCornerRadius > 0)
                {
                    Scrollbar.Image = WobbleAssets.WhiteBox;
                    ScrollbarBackground.Image = WobbleAssets.WhiteBox;
                }

                PreviousScrollbarCornerRadius = 0;
                return;
            }

            if (Scrollbar.Width.Equals(PreviousScrollbarTextureWidth) &&
                Scrollbar.Height.Equals(PreviousScrollbarTextureHeight) &&
                ScrollbarBackground.Height.Equals(PreviousScrollbarTrackTextureHeight) &&
                ScrollbarCornerRadius.Equals(PreviousScrollbarCornerRadius))
                return;

            Scrollbar.Image = RoundedRectTextureCache.Get(Scrollbar.Width, Scrollbar.Height, ScrollbarCornerRadius);
            ScrollbarBackground.Image = RoundedRectTextureCache.Get(ScrollbarBackground.Width, ScrollbarBackground.Height, ScrollbarCornerRadius);
            PreviousScrollbarTextureWidth = Scrollbar.Width;
            PreviousScrollbarTextureHeight = Scrollbar.Height;
            PreviousScrollbarTrackTextureHeight = ScrollbarBackground.Height;
            PreviousScrollbarCornerRadius = ScrollbarCornerRadius;
        }

        /// <summary>
        ///     Returns whether this container currently owns mouse wheel input. Derived overlay
        ///     containers can evaluate their input gate here so capture does not depend on update order.
        /// </summary>
        protected virtual bool IsMouseWheelInputCaptureActive() =>
            CapturesMouseWheelInput && InputEnabled && Visible && !IsDisposed && IsHovered();

        private bool IsMouseWheelInputCapturedByAnotherContainer()
        {
            // Prefer the most recently registered active capturer. Transient overlays are
            // typically created last, and this also keeps overlapping capturers deterministic.
            for (var i = MouseWheelInputCapturers.Count - 1; i >= 0; i--)
            {
                var capturer = MouseWheelInputCapturers[i];
                if (capturer.IsMouseWheelInputCaptureActive())
                    return capturer != this;
            }

            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ContentContainer.Destroy();
            base.Destroy();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            CapturesMouseWheelInput = false;
            base.Dispose();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            var game = GameBase.Game;
            var graphicsDevice = game.GraphicsDevice;

            // Flush the parent batch before changing its scissor rectangle.
            _ = game.TryEndBatch();

            var currentRect = graphicsDevice.ScissorRectangle;
            var currentRasterizerState = graphicsDevice.RasterizerState;
            var previousDrawClip = SpriteBatchOptions.PushClip(ScreenRectangle);

            try
            {
                // Find the width and height scale of the window.
                var widthScale = game.Graphics.PreferredBackBufferWidth / WindowManager.Width;
                var heightScale = game.Graphics.PreferredBackBufferHeight / WindowManager.Height;

                // Calculate the new rectangle taking into account the scaling of the window.
                var left = (int)Math.Floor(ScreenRectangle.Left * widthScale);
                var top = (int)Math.Floor(ScreenRectangle.Top * heightScale);
                var right = (int)Math.Ceiling(ScreenRectangle.Right * widthScale);
                var bottom = (int)Math.Ceiling(ScreenRectangle.Bottom * heightScale);
                var rect = new Rectangle(left, top, right - left, bottom - top);

                if (currentRasterizerState?.ScissorTestEnable == true)
                    rect = Rectangle.Intersect(rect, currentRect);

                // Set new scissor rect to the scaled rect.
                graphicsDevice.ScissorRectangle = Rectangle.Intersect(rect, graphicsDevice.Viewport.Bounds);

                // Draw sprite + children.
                base.Draw(gameTime);
            }
            finally
            {
                // Flush this container before restoring the parent clip.
                _ = game.TryEndBatch();
                SpriteBatchOptions.RestoreClip(previousDrawClip);
                graphicsDevice.ScissorRectangle = currentRect;

                GameBase.DefaultSpriteBatchOptions.Begin();
                GameBase.DefaultSpriteBatchInUse = true;
            }
        }

        /// <summary>
        ///     Adds a drawable that'll be contained in the ScrollContainer.
        /// </summary>
        public void AddContainedDrawable(Drawable drawable)
        {
            AddContainedDrawable(drawable, false);
        }

        /// <summary>
        ///     Adds a drawable and optionally keeps it within the width available beside the scrollbar.
        /// </summary>
        public void AddContainedDrawable(Drawable drawable, bool fillAvailableWidth,
            float leftPadding = 0, float minimumRightPadding = 0, float contentRightPadding = 0)
        {
            drawable.Parent = ContentContainer;

            // Set drawable and children to use the same SpriteBatch
            drawable.UsePreviousSpriteBatchOptions = true;
            drawable.Children.ForEach(x => x.UsePreviousSpriteBatchOptions = true);

            if (!fillAvailableWidth)
                return;

            ContainedDrawableWidths[drawable] = new ContainedDrawableWidthOptions(leftPadding, minimumRightPadding, contentRightPadding);
            ApplyContainedDrawableWidth(drawable, ContainedDrawableWidths[drawable]);
        }

        public void RemoveContainedDrawable(Drawable drawable)
        {
            ContainedDrawableWidths.Remove(drawable);
            drawable.Parent = null;
            drawable.UsePreviousSpriteBatchOptions = false;
            drawable.Children.ForEach(x => x.UsePreviousSpriteBatchOptions = false);
        }

        /// <summary>
        ///     Refreshes the content viewport and the widths of contained fill-width drawables.
        /// </summary>
        public void RefreshContentLayout()
        {
            var width = ContentViewportWidth;
            if (Math.Abs(ContentViewport.Width - width) > 0.001f || Math.Abs(ContentViewport.Height - Height) > 0.001f)
                ContentViewport.Size = new ScalableVector2(width, Height);

            if (Math.Abs(ContentContainer.Width - width) > 0.001f)
                ContentContainer.Width = width;

            List<Drawable> staleDrawables = null;
            foreach (var pair in ContainedDrawableWidths)
            {
                if (pair.Key.IsDisposed || pair.Key.Parent != ContentContainer)
                {
                    staleDrawables ??= new List<Drawable>();
                    staleDrawables.Add(pair.Key);
                    continue;
                }

                ApplyContainedDrawableWidth(pair.Key, pair.Value);
            }

            if (staleDrawables == null)
                return;

            foreach (var drawable in staleDrawables)
                ContainedDrawableWidths.Remove(drawable);
        }

        private void ApplyContainedDrawableWidth(Drawable drawable, ContainedDrawableWidthOptions options)
        {
            if (Math.Abs(drawable.X - options.LeftPadding) > 0.001f)
                drawable.X = options.LeftPadding;

            var right = Math.Min(ContentViewportWidth - options.ContentRightPadding, Width - options.MinimumRightPadding);
            var width = Math.Max(1, right - options.LeftPadding);
            if (Math.Abs(drawable.Width - width) > 0.001f)
                drawable.Width = width;
        }

        private readonly struct ContainedDrawableWidthOptions
        {
            public float LeftPadding { get; }

            public float MinimumRightPadding { get; }

            public float ContentRightPadding { get; }

            public ContainedDrawableWidthOptions(float leftPadding, float minimumRightPadding, float contentRightPadding)
            {
                LeftPadding = leftPadding;
                MinimumRightPadding = minimumRightPadding;
                ContentRightPadding = contentRightPadding;
            }
        }

        /// <summary>
        ///     Scrolls to a given y position.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="time"></param>
        public void ScrollTo(float y, int time)
        {
            // Make sure content container is clamped to the viewport.
            y = MathHelper.Clamp(y, -ContentContainer.Height + Height, 0);
            TargetY = y;
            PreviousTargetY = y;

            ContentContainer.Animations.Clear();
            ContentContainer.Animations.Add(new Animation(AnimationProperty.Y, EasingType, ContentContainer.Y, y, time));
        }
    }
}
