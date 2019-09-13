using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    public class ScrollContainer : Sprite
    {
        /// <summary>
        ///     The content that holds and is a parent of all sprites
        /// </summary>
        public Container ContentContainer { get; }

        /// <summary>
        ///     The scroll bar
        /// </summary>
        public Sprite Scrollbar { get; }

        /// <summary>
        ///     The target y position of the container.
        /// </summary>
        public float TargetY { get; set;  }

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
        ///     Determines if the scrolling input is enabled for the container.
        /// </summary>
        public bool InputEnabled { get; set; }

        /// <summary>
        ///     The minimum y the scrollbar will be clamped to
        /// </summary>
        protected int MinScrollBarY { get; set; }

        /// <summary>
        ///     If the container allows fast scrolling with the middle mouse button
        /// </summary>
        public bool AllowMiddleMouseDragging { get; set; } = true;

        /// <summary>
        ///     The scroll speed used when the user is holding down the middle mouse button
        /// </summary>
        public int TimeToCompleteMiddleMouseScroll { get; set; } = 600;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScrollContainer(ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false)
        {
            Size = size;

            // Create the SpriteBatchOptions with scissor rect enabled.
            SpriteBatchOptions = new SpriteBatchOptions
            {
                SortMode = SpriteSortMode.Immediate,
                BlendState = BlendState.NonPremultiplied,
                RasterizerState = new RasterizerState
                {
                    ScissorTestEnable = true,
                },
            };

            // Create container in which all scrolling contents will be children of.
            ContentContainer = new Container(contentSize, new ScalableVector2(0, 0))
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };

            // Choose starting location of the scroll container
            ContentContainer.Y = startFromBottom ? -ContentContainer.Height : 0;

            // Create the scroll bar.
            Scrollbar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Width = 15,
                Tint = Color.Black,
                X = 1
            };

            TargetY = ContentContainer.Y;
            PreviousTargetY = TargetY;
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Set scrollbar heigt.
            Scrollbar.Height = Height / ContentContainer.Height * Height;

            // Set min scroll height to 30.
            if (Scrollbar.Height < 30)
                Scrollbar.Height = 30;

            // Handle scrolling
            if (InputEnabled)
            {
                // Middle mouse scrolling
                if (IsHovered() && AllowMiddleMouseDragging && MouseManager.CurrentState.MiddleButton == ButtonState.Pressed)
                {
                    var percent = MathHelper.Clamp((MouseManager.CurrentState.Y - ScreenRectangle.Y) / ScreenRectangle.Height, 0, 1);
                    TargetY = -ContentContainer.Height * percent;
                }
                else if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                    TargetY += ScrollSpeed;
                else if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                    TargetY -= ScrollSpeed;
                else if (KeyboardManager.IsUniqueKeyPress(Keys.PageUp))
                    TargetY += ScrollSpeed * 5;
                else if (KeyboardManager.IsUniqueKeyPress(Keys.PageDown))
                    TargetY -= ScrollSpeed * 5;
            }

            // Make sure content container is clamped to the viewport.
            TargetY = MathHelper.Clamp(TargetY, -ContentContainer.Height + Height, 0);

            // Calculate the scrollbar's y position.
            var percentage = Math.Abs(-ContentContainer.Y / (-ContentContainer.Height + Height) * 100);
            Scrollbar.Y = percentage / 100 * (Height - Scrollbar.Height) - (Height - Scrollbar.Height);

            if (Scrollbar.Y < MinScrollBarY)
                Scrollbar.Y = MinScrollBarY;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (TargetY != PreviousTargetY)
            {
                ContentContainer.Animations.Clear();

                var timeToComplete = MouseManager.CurrentState.MiddleButton == ButtonState.Pressed
                    ? TimeToCompleteMiddleMouseScroll
                    : TimeToCompleteScroll;

                ContentContainer.Animations.Add(new Animation(AnimationProperty.Y, EasingType,
                                                            ContentContainer.Y, TargetY, timeToComplete));
            }

            PreviousTargetY = TargetY;
            base.Update(gameTime);
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
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Get the current scissor rectangle and save it so that we can reset it back.
            // to its original.
            var currentRect = GameBase.Game.GraphicsDevice.ScissorRectangle;

            // Find the width and height scale of the window.
            var widthScale = GameBase.Game.Graphics.PreferredBackBufferWidth / WindowManager.Width;
            var heightScale = GameBase.Game.Graphics.PreferredBackBufferHeight / WindowManager.Height;

            // Calculate the new rectangle taking into account the scaling of the window.
            var rect = new Rectangle()
            {
                X = (int)(ScreenRectangle.X * widthScale),
                Y = (int)(ScreenRectangle.Y * heightScale),
                Width = (int)(ScreenRectangle.Width * widthScale),
                Height = (int)(ScreenRectangle.Height * heightScale),
            };

            // Set new scissor rect to the scaled rect.
            GameBase.Game.GraphicsDevice.ScissorRectangle = rect;

            // Draw sprite + children.
            base.Draw(gameTime);

            // Reset scissor rect back to original
            GameBase.Game.GraphicsDevice.ScissorRectangle = currentRect;
        }

        /// <summary>
        ///     Adds a drawable that'll be contained in the ScrollContainer.
        /// </summary>
        public void AddContainedDrawable(Drawable drawable)
        {
            drawable.Parent = ContentContainer;

            // Set drawable and children to use the same SpriteBatch
            drawable.UsePreviousSpriteBatchOptions = true;
            drawable.Children.ForEach(x => x.UsePreviousSpriteBatchOptions = true);
        }

        public void RemoveContainedDrawable(Drawable drawable)
        {
            drawable.Parent = null;
            drawable.UsePreviousSpriteBatchOptions = false;
            drawable.Children.ForEach(x => x.UsePreviousSpriteBatchOptions = false);
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