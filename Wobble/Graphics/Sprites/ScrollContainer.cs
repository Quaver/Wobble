using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Bindables;
using Wobble.Graphics.Transformations;
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
        private float TargetY { get; set;  }

        /// <summary>
        ///     The target y position in the previous frame.
        /// </summary>
        private float PreviousTargetY { get; set; }

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScrollContainer(ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false)
        {
            Size = size;

            // Create the SpriteBatchOptions with scissor rect enabled.
            SpriteBatchOptions = new SpriteBatchOptions
            {
                RasterizerState = new RasterizerState { ScissorTestEnable = true }
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
            if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                TargetY += ScrollSpeed;
            else if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                TargetY -= ScrollSpeed;

            // Make sure content container is clamped to the viewport.
            TargetY = MathHelper.Clamp(TargetY, -ContentContainer.Height + Height, 0);

            // Calculate the scrollbar's y position.
            var percentage = -ContentContainer.Y / ( -ContentContainer.Height + Height ) * 100;
            Scrollbar.Y = percentage / 100 * (Height - Scrollbar.Height);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (TargetY != PreviousTargetY)
            {
                ContentContainer.Transformations.Clear();
                ContentContainer.Transformations.Add(new Transformation(TransformationProperty.Y, EasingType,
                                                            ContentContainer.Y, TargetY, TimeToCompleteScroll));
            }

            PreviousTargetY = TargetY;
            base.Update(gameTime);
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
            var rect = ScreenRectangle.ToRectangle();
            rect.X = (int)(rect.X * widthScale);
            rect.Y = (int)(rect.Y * heightScale);
            rect.Width = (int)(rect.Width * widthScale);
            rect.Height = (int)(rect.Height * heightScale);

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

            ContentContainer.Transformations.Clear();
            ContentContainer.Transformations.Add(new Transformation(TransformationProperty.Y, EasingType, ContentContainer.Y, y, time));
        }
    }
}