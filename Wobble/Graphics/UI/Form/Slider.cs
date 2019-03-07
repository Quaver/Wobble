using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Wobble.Graphics.UI.Form
{
    public class Slider : Button
    {
         /// <summary>
        ///     Global property that dictates if **any** slider is actually held.
        /// </summary>
        public static bool SliderAlreadyHeld { get; set; }

        /// <summary>
        ///     The value that this slider binds to.
        /// </summary>
        public BindableInt BindedValue { get; }

        /// <summary>
        ///     The progress slider image.
        /// </summary>
        public Sprite ProgressBall { get; }

        /// <summary>
        ///     If the mouse is held down and hasn't let go yet.
        /// </summary>
        public bool MouseInHoldSequence { get; set; }

        /// <summary>
        ///     If the slider is vertical or not.
        ///         True = Vertical
        ///         False = Horizontal
        /// </summary>
        private bool IsVertical { get; }

        /// <summary>
        ///     The last normalized slider value in [0, 1].
        /// </summary>
        private float LastValue { get; set; }

        /// <summary>
        ///     The mouse state of the previous frame.
        /// </summary>
        private MouseState PreviousMouseState { get; set; }

        /// <summary>
        ///     The original size of the progress image
        /// </summary>
        private ScalableVector2 ProgressBallSize { get; } = new ScalableVector2(15, 15);

        /// <summary>
        ///     The previous value that we have stored.
        /// </summary>
        private int PreviousValue { get; set; }

         /// <summary>
        ///     The ball position normalized to [0, 1].
        /// </summary>
        private float NormalizedBallPosition => (float) (BindedValue.Value - BindedValue.MinValue) / (BindedValue.MaxValue - BindedValue.MinValue);

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new SliderButton. Takes in a BindableInt as an argument.
        /// </summary>
        /// <param name="binded"></param>
        /// <param name="size"></param>
        /// <param name="progressBall"></param>
        /// <param name="vertical"></param>
        public Slider(BindableInt binded, Vector2 size, Texture2D progressBall, bool vertical = false)
        {
            BindedValue = binded;
            IsVertical = vertical;

            Width = size.X;
            Height = size.Y;
            Tint = Color.White;

            // Create the progress sliding thing.
            ProgressBall = new Sprite()
            {
                Alignment = IsVertical ? Alignment.TopCenter : Alignment.MidLeft,
                Image = progressBall,
                Size = ProgressBallSize,
                Tint = Color.White,
                Parent = this
            };

            // Whenever the value changes, we need to update the slider accordingly,
            // so hook onto this event with a handler.
            BindedValue.ValueChanged += OnValueChanged;
        }

         /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(GameTime gameTime)
        {
            if (MouseManager.CurrentState.LeftButton == ButtonState.Released)
            {
                MouseInHoldSequence = false;
                SliderAlreadyHeld = false;
            }

            if (IsHeld)
                MouseHeld();

            // Handle the changing of the value for this button.
            if (MouseInHoldSequence)
                HandleSliderValueChanges();
            else if (IsMouseInClickArea())
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                    BindedValue.Value--;

                if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                    BindedValue.Value++;
            }

            PreviousMouseState = Mouse.GetState();
            SetProgressPosition(gameTime.ElapsedGameTime.TotalMilliseconds);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Destroy
        /// </summary>
        public override void Destroy()
        {
            // Remove the event handler for the binded value.
            // ReSharper disable once DelegateSubtraction
            BindedValue.ValueChanged -= OnValueChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Gets the new value of the slider and sets it to the binded value.
        /// </summary>
        private void HandleSliderValueChanges()
        {
            // Compute the normalized value in [0, 1].
            float value;
            if (IsVertical)
                value = 1 - (MouseManager.CurrentState.Y - AbsolutePosition.Y) / AbsoluteSize.Y;
            else
                value = (MouseManager.CurrentState.X - AbsolutePosition.X) / AbsoluteSize.X;

            // If the value is 0 or lower, set the binded value to the minimum.
            if (value <= 0 && LastValue > 0)
                BindedValue.Value = BindedValue.MinValue;
            // If the value is 1 or higher set the binded value to the maximum.
            else if (value >= 1 && LastValue < 1)
                BindedValue.Value = BindedValue.MaxValue;
            // If the value is anything else, set it accordingly.
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (value > 0 && value < 1 && LastValue != value)
                BindedValue.Value = (int)(value * (BindedValue.MaxValue - BindedValue.MinValue) + BindedValue.MinValue);

            LastValue = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the click area of the slider.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            // The RectY increase of the click area.
            const int offset = 40;

            DrawRectangle clickArea;

            if (IsVertical)
                clickArea = new DrawRectangle(ScreenRectangle.X - offset / 2f, ScreenRectangle.Y, ScreenRectangle.Width + offset, ScreenRectangle.Height);
            else
                clickArea = new DrawRectangle(ScreenRectangle.X, ScreenRectangle.Y - offset / 2f, ScreenRectangle.Width, ScreenRectangle.Height + offset);

            return GraphicsHelper.RectangleContains(clickArea, MouseManager.CurrentState.Position);
        }

        /// <summary>
        ///     When the button is moused over and held down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseHeld()
        {
            if (SliderAlreadyHeld || PreviousMouseState.LeftButton != ButtonState.Pressed)
                return;

            MouseInHoldSequence = true;
            SliderAlreadyHeld = true;
        }

        /// <summary>
        ///     Changes the color of both the slider line + progress ball.
        /// </summary>
        /// <param name="color"></param>
        public void ChangeColor(Color color)
        {
            Tint = color;
            ProgressBall.Tint = color;
        }

        /// <summary>
        ///     Sets the correct position of the progress image
        /// </summary>
        private void SetProgressPosition(double dt)
        {
            if (IsVertical)
            {
                ProgressBall.Y = MathHelper.Lerp(ProgressBall.Y,
                    (1 - NormalizedBallPosition) * Height - ProgressBall.Height / 2,
                    (float) Math.Min(dt / 30, 1));
            }
            else
            {
                ProgressBall.X = MathHelper.Lerp(ProgressBall.X,
                    NormalizedBallPosition * Width - ProgressBall.Width / 2,
                    (float) Math.Min(dt / 30, 1));
            }
        }

        /// <summary>
        ///     Plays a sound effect at a given value based on the previously captured binded val.
        /// </summary>
        /// <param name="val"></param>
        private void PlaySoundEffectWhenChanged(int val)
        {
            // Set the min and max based on the direction we're going.
            var max = val > PreviousValue ? 1f : 0;
            var min = val > PreviousValue ? 0 : -1f;
        }

        /// <summary>
        ///     This method is an event handler specifically for handling the case of when the value of the binded value
        ///     has changed. This will automatically set the progress position.
        ///
        ///     This is mainly for cases such as volume, where it can be controlled through means other than the slider
        ///     (example: keybinds), and if the slider is displayed, it should update as well.
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValueChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            // Play a sound effect
            //PlaySoundEffectWhenChanged(e.Value);

            // Update the previous value.
            PreviousValue = e.Value;
            LastValue = NormalizedBallPosition;
        }
    }
}