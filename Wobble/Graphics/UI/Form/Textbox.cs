using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI.Form
{
    /// <summary>
    ///        Textbox used for typing text into.
    /// </summary>
    public class Textbox : ScrollContainer
    {
        /// <summary>
        ///     The style of the text box.
        /// </summary>
        public TextboxStyle Style { get; }

        /// <summary>
        ///     The text that is currently displayed
        /// </summary>
        public SpriteText InputText { get; }

        /// <summary>
        ///     The cursor that displays where the text input currently is.
        /// </summary>
        public Sprite Cursor { get; }

        /// <summary>
        ///     The raw text for this sprite.
        /// </summary>
        private string _rawText;
        public string RawText
        {
            get => _rawText;
            set
            {
                _rawText = value;
                InputText.Text = value;
            }
        }

        /// <summary>
        ///     Action called when pressing enter and submitting the text box.
        /// </summary>
        public Action<string> OnSubmit { get; }

        /// <summary>
        ///     The time since the cursor's visiblity has changed.
        /// </summary>
        private double TimeSinceCursorVisibllityChanged { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="style"></param>
        /// <param name="size"></param>
        /// <param name="font"></param>
        /// <param name="initialText"></param>
        /// <param name="textScale"></param>
        /// <param name="onSubmit"></param>
        public Textbox(TextboxStyle style, ScalableVector2 size, SpriteFont font, string initialText = "", float textScale = 1.0f,
            Action<string> onSubmit = null) : base(size, size)
        {
            _rawText = initialText;
            Style = style;

            InputText = new SpriteText(font, RawText, size, textScale)
            {
                TextAlignment = Alignment.TopLeft,
                X = 5,
                Y = 2,
                Padding = 5
            };

            switch (Style)
            {
                case TextboxStyle.MultiLine:
                    InputText.Style = TextStyle.WordwrapMultiLine;
                    break;
                case TextboxStyle.SingleLine:
                    InputText.Style = TextStyle.OverflowSingleLine;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Cursor = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(2, InputText.MeasureString("A").Y), // Height is equivalent to text height.
                Tint = Color.White,
                Y = 2
            };

            CalculateContainerX();
            ChangeCursorLocation();
            AddContainedDrawable(InputText);
            AddContainedDrawable(Cursor);
            GameBase.Game.Window.TextInput += OnTextInputEntered;
            OnSubmit += onSubmit;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformCursorBlinking(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            GameBase.Game.Window.TextInput -= OnTextInputEntered;
            base.Destroy();
        }

        /// <summary>
        ///     When text is entered in the box, this'll run to update the text sprite.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextInputEntered(object sender, TextInputEventArgs e)
        {
            // Handle key inputs.
            switch (e.Key)
            {
                // Ignore these keys
                case Keys.Tab:
                case Keys.Delete:
                case Keys.Escape:
                    break;
                // Back spacing
                case Keys.Back:
                    if (string.IsNullOrEmpty(RawText))
                        return;

                    RawText = RawText.TrimEnd(RawText[RawText.Length - 1]);
                    break;
                // On Submit
                case Keys.Enter:
                    if (string.IsNullOrEmpty(RawText))
                        return;

                    // Run the callback function that was passed in.
                    OnSubmit?.Invoke(RawText);

                    // Clear text box.
                    RawText = "";
                    break;
                // Input text
                default:
                    RawText += e.Character;
                    break;
            }

            CalculateContainerX();
            ChangeCursorLocation();

            // Make cursor visible and reset its visiblity changing.
            Cursor.Visible = true;
            TimeSinceCursorVisibllityChanged = 0;
        }

        /// <summary>
        ///    If it's a single lined textbox, then we need to move the ContentContainer (Viewinew container),
        ///     either to the left or two the right depending on how large the text is.
        /// </summary>
        private void CalculateContainerX()
        {
            if (Style != TextboxStyle.SingleLine)
                return;

            var size = InputText.MeasureString();
            ContentContainer.X = size.X > Width ? Width - size.X - InputText.Padding - Cursor.Width: 0;
        }

        /// <summary>
        ///     Changes the location of the cursor to the position of where the text is.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ChangeCursorLocation()
        {
            switch (Style)
            {
                case TextboxStyle.MultiLine:
                    break;
                case TextboxStyle.SingleLine:
                    Cursor.X = InputText.MeasureString().X + 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Makes the cursor blink
        /// </summary>
        private void PerformCursorBlinking(GameTime gameTime)
        {
            TimeSinceCursorVisibllityChanged += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!(TimeSinceCursorVisibllityChanged >= 500))
                return;

            Cursor.Visible = !Cursor.Visible;
            TimeSinceCursorVisibllityChanged = 0;
        }
    }
    /// <summary>
    ///     The type of text box
    /// </summary>
    public enum TextboxStyle
    {
        // Wraps to multiple lines in the box.
        MultiLine,

        // Stays on a single line and moves the viewing container as the user types.
        SingleLine
    }
}