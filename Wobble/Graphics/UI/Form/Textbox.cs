using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Platform;
using Wobble.Platform.Windows;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Wobble.Graphics.UI.Form
{
    /// <summary>
    ///        Textbox used for typing text into.
    /// </summary>
    public class Textbox : ScrollContainer
    {
        /// <summary>
        ///     The text that is currently displayed
        /// </summary>
        public SpriteTextPlus InputText { get; }

        /// <summary>
        ///     The cursor that displays where the text input currently is.
        /// </summary>
        public Sprite Cursor { get; }

        /// <summary>
        ///     When the text is selected (CTRL + A), this sprite will display
        ///     and make it look as if the text box is selected.
        /// </summary>
        public Sprite SelectedSprite { get; }

        /// <summary>
        ///     The button for the text box to control if it is focused or not.
        /// </summary>
        public ImageButton Button { get; }

        /// <summary>
        ///     Regular expression for <see cref="RawText"/>
        /// </summary>
        public Regex AllowedCharacters { get; set; } = new Regex("(.*?)");

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

                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(PlaceholderText))
                {
                    InputText.Text = PlaceholderText;
                    InputText.Alpha = 0.50f;
                }
                else
                {
                    InputText.Text = value;
                    InputText.Alpha = 1;
                }
            }
        }

        /// <summary>
        ///     The text used as a placeholder.
        /// </summary>
        public string PlaceholderText { get; set; }

        /// <summary>
        ///     Maximum amount of characters that could be in the textbox.
        /// </summary>
        public int MaxCharacters { get; set; } = int.MaxValue;

        /// <summary>
        ///     If the textbox is focused, it will handle input, if not, it wont.
        /// </summary>
        private bool _focused = false;
        public bool Focused
        {
            get => AlwaysFocused || _focused;
            set => _focused = value;
        }

        /// <summary>
        ///     If set to true, the textbox will always be focused.
        /// </summary>
        public bool AlwaysFocused { get; set; }

        /// <summary>
        ///     Determines if the text is selected. (CTRL+A) state
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        ///     Action called when pressing enter and submitting the text box.
        /// </summary>
        public Action<string> OnSubmit { get; set; }

        /// <summary>
        ///     Action called when the user stops typing.
        /// </summary>
        public Action<string> OnStoppedTyping { get; set; }

        /// <summary>
        ///     The time since the cursor's visiblity has changed.
        /// </summary>
        private double TimeSinceCursorVisibllityChanged { get; set; }

        /// <summary>
        ///     The amount of time in milliseconds it'll take before firing OnStoppedTyping
        /// </summary>
        public int StoppedTypingActionCalltime { get; set; } = 500;

        /// <summary>
        ///     If true, it'll allow the textbox to be submitted.
        /// </summary>
        public bool AllowSubmission { get; set; } = true;

        /// <summary>
        ///     The amount of time since the user has stopped typing, so that
        ///     we can perform actions after they've stopped typing.
        /// </summary>
        private double TimeSinceStoppedTyping { get; set; }

        /// <summary>
        ///     When the user stops typing after a while, this variable tracks if we've already fired
        ///     the action handlers, to prevent doing it from every frame.
        ///
        ///     Set to true by default because we don't want to call on a just initialized Textbox.
        /// </summary>
        private bool FiredStoppedTypingActionHandlers { get; set; } = true;

        /// <summary>
        ///     Clipboard for the windows instance.
        /// </summary>
        private Clipboard Clipboard { get; } = Clipboard.NativeClipboard;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="font"></param>
        /// <param name="fontSize"></param>
        /// <param name="initialText"></param>
        /// <param name="placeHolderText"></param>
        /// <param name="onSubmit"></param>
        /// <param name="onStoppedTyping"></param>
        public Textbox(ScalableVector2 size, WobbleFontStore font, int fontSize,
            string initialText = "", string placeHolderText = "",  Action<string> onSubmit = null, Action<string> onStoppedTyping = null)
            : base(size, size)
        {
            PlaceholderText = placeHolderText;
            _rawText = initialText;

            InputText = new SpriteTextPlus(font, RawText, fontSize)
            {
                X = 10,
                Alignment = Alignment.MidLeft
            };

            if (!string.IsNullOrEmpty(initialText))
                RawText = initialText;
            else if (!string.IsNullOrEmpty(placeHolderText))
            {
                InputText.Text = placeHolderText;
                InputText.Alpha = 0.50f;
            }

            Cursor = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(2, InputText.Height), // Height is equivalent to text height.
                Tint = Color.White,
                Visible = false
            };

            SelectedSprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(Width * 0.98f, Height * 0.85f),
                Tint = Color.White,
                Alpha = 0,
                Y = 1,
                X = InputText.X - 1
            };

            // Create the invisible button that will dictate if the button is focused or not.
            Button = new ImageButton(WobbleAssets.WhiteBox, (o, e) => Focused = true)
            {
                Parent = this,
                Size = Size,
                Alpha = 0
            };

            // If the user clicks outside of the button, then it won't be focused anymore.
            Button.ClickedOutside += (o, e) =>
            {
                Focused = false;
            };

            CalculateContainerX();
            ChangeCursorLocation();

            AddContainedDrawable(InputText);
            AddContainedDrawable(Cursor);
            AddContainedDrawable(SelectedSprite);

            GameBase.Game.Window.TextInput += OnTextInputEntered;
            OnSubmit += onSubmit;
            OnStoppedTyping += onStoppedTyping;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeSinceStoppedTyping += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Handle when the user stops typing. and invoke the action handlers.
            if (TimeSinceStoppedTyping >= StoppedTypingActionCalltime && !FiredStoppedTypingActionHandlers)
            {
                OnStoppedTyping?.Invoke(RawText);
                FiredStoppedTypingActionHandlers = true;
            }

            // Handle all input.
            HandleCtrlInput();
            HandleEnter();
            CalculateContainerX();
            ChangeCursorLocation();

            // Change the alpha of the selected sprite depending on if we're currently in a CTRL+A operation.
            SelectedSprite.Alpha = MathHelper.Lerp(SelectedSprite.Alpha, Selected ? 0.25f : 0,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

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
            if (!Focused)
                return;

            // On Linux this gets sent on switching the keyboard layout.
            if (e.Character == '\0')
                return;

            // On Linux some characters (like Backspace, plus or minus) get sent here even when CTRL is down, and we
            // don't handle that here.
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl)
                || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            // Enter is handled in Update() because TextInput only receives the regular Enter and not the NumPad Enter.
            if (e.Key == Keys.Enter)
                return;

            // If the text is selected (in a CTRL+A) operation
            if (Selected)
            {
                // Clear text
                RawText = "";

                switch (e.Key)
                {
                    case Keys.Back:
                    case Keys.Tab:
                    case Keys.Delete:
                    case Keys.VolumeUp:
                    case Keys.VolumeDown:
                        break;
                    // For all other key presses, we reset the string and append the new character
                    default:
                        if (RawText.Length + 1 <= MaxCharacters)
                        {
                            var proposedText = RawText + e.Character;

                            if (!AllowedCharacters.IsMatch(proposedText))
                                return;

                            RawText += proposedText;
                        }
                        break;
                }

                Selected = false;
            }
            // Handle normal key presses.
            else
            {
                // Handle key inputs.
                switch (e.Key)
                {
                    // Ignore these keys
                    case Keys.Tab:
                    case Keys.Delete:
                    case Keys.Escape:
                    case Keys.VolumeUp:
                    case Keys.VolumeDown:
                        return;
                    // Back spacing
                    case Keys.Back:
                        if (string.IsNullOrEmpty(RawText))
                            return;

                        var charStartIndices = StringInfo.ParseCombiningCharacters(RawText);
                        RawText = RawText.Remove(charStartIndices.Last());
                        break;
                    // Input text
                    default:
                        if (RawText.Length + 1 <= MaxCharacters)
                        {
                            var proposedText = RawText + e.Character;

                            if (!AllowedCharacters.IsMatch(proposedText))
                                return;

                            RawText = proposedText;
                        }
                        break;
                }
            }

            ReadjustTextbox();
        }

        /// <summary>
        ///    If it's a single lined textbox, then we need to move the ContentContainer (Viewinew container),
        ///     either to the left or two the right depending on how large the text is.
        /// </summary>
        private void CalculateContainerX()
        {
            ContentContainer.X = InputText.Width > Width ? Width - InputText.Width - Cursor.Width - 10: 0;
        }

        /// <summary>
        ///     Changes the location of the cursor to the position of where the text is.
        /// </summary>
        private void ChangeCursorLocation()
        {
            if (string.IsNullOrEmpty(RawText))
            {
                Cursor.X = InputText.X;
                return;
            }

            Cursor.X = InputText.X + InputText.Width;
            SelectedSprite.Width = Cursor.X;
        }

        /// <summary>
        ///     Makes the cursor blink
        /// </summary>
        private void PerformCursorBlinking(GameTime gameTime)
        {
            if (!Focused)
            {
                Cursor.Visible = false;
                return;
            }

            TimeSinceCursorVisibllityChanged += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!(TimeSinceCursorVisibllityChanged >= 500))
                return;

            Cursor.Visible = !Cursor.Visible;
            TimeSinceCursorVisibllityChanged = 0;
        }

        /// <summary>
        ///     Makes sure the textbox cursor and x is all up-to-date after entering/removing text.
        /// </summary>
        public void ReadjustTextbox()
        {
            // Make cursor visible and reset its visiblity changing.
            Cursor.Visible = true;
            TimeSinceCursorVisibllityChanged = 0;
            TimeSinceStoppedTyping = 0;

            FiredStoppedTypingActionHandlers = false;
        }

        /// <summary>
        ///     Handles control input for the textbox.
        /// </summary>
        private void HandleCtrlInput()
        {
            // Make sure the textbox is focused and that the control buttons are down before handling anything.
            if (!Focused || (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl)
                && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl)))
                return;

            // CTRL+A, Select the text.
            if (KeyboardManager.IsUniqueKeyPress(Keys.A) && !string.IsNullOrEmpty(RawText))
                Selected = true;

            // CTRL+C, Copy the text to the clipboard.
            if (KeyboardManager.IsUniqueKeyPress(Keys.C) && Selected)
                Clipboard.SetText(RawText);

            // CTRL+X, Cut the text to the clipboard.
            if (KeyboardManager.IsUniqueKeyPress(Keys.X) && Selected)
            {
                Clipboard.SetText(RawText);
                RawText = "";

                ReadjustTextbox();
                Selected = false;
            }

            // CTRL+V Paste text
            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
            {
                var clipboardText = Clipboard.GetText().Replace("\n", "").Replace("\r", "");

                if (!string.IsNullOrEmpty(clipboardText))
                {
                    if (Selected)
                    {
                        if (!AllowedCharacters.IsMatch(clipboardText))
                            return;

                        RawText = clipboardText;
                    }
                    else
                    {
                        var proposed = RawText + clipboardText;

                        if (!AllowedCharacters.IsMatch(proposed))
                            return;

                        RawText = proposed;
                    }
                }

                ReadjustTextbox();
                Selected = false;
            }

            // CTRL+W or CTRL+Backspace: kill word backwards.
            // This means killing all trailing whitespace and then all trailing non-whitespace.
            if (KeyboardManager.IsUniqueKeyPress(Keys.W) || KeyboardManager.IsUniqueKeyPress(Keys.Back))
            {
                var withoutTrailingWhitespace = RawText.TrimEnd();
                var nonWhitespacesInTheEnd = withoutTrailingWhitespace.ToCharArray()
                    .Select(c => c).Reverse().TakeWhile(c => !char.IsWhiteSpace(c)).Count();
                RawText = withoutTrailingWhitespace.Substring(0,
                    withoutTrailingWhitespace.Length - nonWhitespacesInTheEnd);

                ReadjustTextbox();
                Selected = false;
            }

            // Ctrl+U: kill line backwards.
            // Delete from the cursor position to the start of the line.
            if (KeyboardManager.IsUniqueKeyPress(Keys.U))
            {
                // Since we don't have a concept of a cursor, simply delete the whole text.
                RawText = "";

                ReadjustTextbox();
                Selected = false;
            }
        }

        /// <summary>
        ///     Handles the Enter button (both regular and numpad) for the textbox.
        /// </summary>
        private void HandleEnter()
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
            {
                if (!AllowSubmission)
                    return;

                if (string.IsNullOrEmpty(RawText))
                    return;

                // Run the callback function that was passed in.
                OnSubmit?.Invoke(RawText);

                // Clear text box.
                RawText = "";
                Selected = false;
                ReadjustTextbox();
            }
        }
    }
}