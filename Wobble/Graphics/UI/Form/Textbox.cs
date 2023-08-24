﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Audio.Samples;
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

        public string SelectedRawText => RawText.Substring(SelectedPart.start, SelectedPart.end - SelectedPart.start);

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
        ///     Determines the part of the text that is selected.
        /// </summary>
        public (int start, int end) SelectedPart { get; private set; }

        /// <summary>
        ///     The position of the cursor when the selection begins.
        /// </summary>
        private int SelectionBegin { get; set; }

        /// <summary>
        ///    The position of the cursor in the textbox. In amount of characters from the start.
        /// </summary>
        public int CursorPosition { get; private set; }

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
        public double TimeSinceCursorVisibllityChanged { get; set; }

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

        /// <summary>
        ///		List of AudioSamples to use for textbox keyclick sound effects.
        ///	</summary>
        private static List<AudioSample> _keyClickSamples;
        public static List<AudioSample> KeyClickSamples
        {
            get => _keyClickSamples;
            set
            {
                _keyClickSamples?.ForEach(x => x.Dispose());
                _keyClickSamples = value;
            }
        }

        /// <summary>
        ///		When enabled, key presses when focusing a textbox will play a randomly selected sfx
        ///		from SkinStore#SoundMenuKeyClicks
        ///	</summary>
        private bool EnableKeyClickSounds { get; set; } = true;

        /// <summary>
        ///		Random Number Generator
        ///	</summary>
        private Random Rng = new Random();

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
            PlaceholderText = placeHolderText ?? "";
            _rawText = initialText ?? "";

            InputText = new SpriteTextPlus(font, RawText, fontSize, false)
            {
                X = 10,
                Alignment = Alignment.MidLeft,
            };

            if (!string.IsNullOrEmpty(initialText))
                RawText = initialText;
            else if (!string.IsNullOrEmpty(placeHolderText))
            {
                InputText.Text = placeHolderText;
                InputText.Alpha = 0.50f;
            }

            CursorPosition = RawText.Length;

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
            UpdateSelectedSprite();

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
            HandleArrowKeys();
            HandleCtrlInput();
            HandleEnter();
            CalculateContainerX();
            ChangeCursorLocation();

            // Change the alpha of the selected sprite depending on if we're currently in a CTRL+A operation.
            SelectedSprite.Alpha = MathHelper.Lerp(SelectedSprite.Alpha, Selected ? 0.5f : 0,
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

            // If the text is selected
            if (Selected)
            {
                RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                CursorPosition = SelectedPart.start;

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
                            var proposedText = RawText.Substring(0, CursorPosition) + e.Character +
                                               RawText.Substring(CursorPosition, RawText.Length - CursorPosition);

                            if (!AllowedCharacters.IsMatch(proposedText))
                                return;

                            RawText = proposedText;
                            CursorPosition++;
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
                        if (CursorPosition == 0)
                            return;

                        var upToCursor = RawText.Substring(0, CursorPosition);
                        var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);
                        upToCursor = upToCursor.Remove(upToCursor.Length - 1);
                        RawText = upToCursor + afterCursor;
                        CursorPosition--;
                        PlayKeyClickSound();
                        break;
                    // Input text
                    default:
                        if (RawText.Length + 1 <= MaxCharacters)
                        {
                            var proposedText = RawText.Substring(0, CursorPosition) + e.Character +
                                               RawText.Substring(CursorPosition, RawText.Length - CursorPosition);

                            if (!AllowedCharacters.IsMatch(proposedText))
                                return;

                            RawText = proposedText;
                            CursorPosition++;

                            PlayKeyClickSound();
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
            ContentContainer.Width = InputText.Width;
            ContentContainer.X = InputText.Width > Width ? Width - InputText.Width - Cursor.Width - 20 : 0;
        }

        /// <summary>
        ///     Changes the location of the cursor to the position of where the text is.
        /// </summary>
        private void ChangeCursorLocation()
        {
            var substring = RawText.Substring(0, CursorPosition);
            var x = InputText.Font.Store.MeasureString(substring).X;

            Cursor.X = x + InputText.X;
        }
        private void UpdateSelectedSprite()
        {
            SelectedSprite.Visible = Selected;
            var startSubstring = RawText.Substring(0, SelectedPart.start);
            var selectedSubstring = RawText.Substring(SelectedPart.start, SelectedPart.end - SelectedPart.start);
            var x = InputText.Font.Store.MeasureString(startSubstring).X;
            var width = InputText.Font.Store.MeasureString(selectedSubstring).X;

            SelectedSprite.X = x + InputText.X;
            SelectedSprite.Width = width;
            Console.WriteLine($"X: {SelectedSprite.X}, Width: {SelectedSprite.Width}");
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

        public void ReadjustCursor()
        {
            Cursor.Visible = true;
            TimeSinceCursorVisibllityChanged = 0;
        }

        private void HandleArrowKeys()
        {
            if (!Focused)
                return;

            KeyboardManager.CurrentState.GetPressedKeys().ToList().ForEach(x => Console.WriteLine(x));

            var shift = KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift);
            var ctrl = KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl);

            var oldCursorPosition = CursorPosition;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                if (ctrl)
                {
                    var upToCursor = RawText.Substring(0, CursorPosition);
                    var nonWhitespacesInTheEnd = upToCursor.ToCharArray()
                        .Select(c => c).Reverse().TakeWhile(c => !char.IsWhiteSpace(c)).Count();
                    CursorPosition = upToCursor.Length - nonWhitespacesInTheEnd;
                }
                else
                {
                    CursorPosition = Math.Max(0, CursorPosition - 1);
                }

                if (shift)
                {
                    SetSelectedPart(oldCursorPosition);
                }

                ReadjustCursor();
                UpdateSelectedSprite();
            }
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                if (ctrl)
                {
                    var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);
                    var nonWhitespacesInTheStart = afterCursor.ToCharArray()
                        .Select(c => c).TakeWhile(c => !char.IsWhiteSpace(c)).Count();
                    CursorPosition = CursorPosition + nonWhitespacesInTheStart;
                }
                else
                {
                    CursorPosition = Math.Min(RawText.Length, CursorPosition + 1);
                }

                if (shift)
                {
                    SetSelectedPart(oldCursorPosition);
                }

                ReadjustCursor();
                UpdateSelectedSprite();
            }
            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                var upToCursor = RawText.Substring(0, CursorPosition);
                var nonNewlinesInTheEnd = upToCursor.ToCharArray()
                    .Select(c => c).Reverse().TakeWhile(c => c != '\n').Count();
                CursorPosition = upToCursor.Length - nonNewlinesInTheEnd;

                if (shift)
                {
                    SetSelectedPart(oldCursorPosition);
                }

                ReadjustCursor();
                UpdateSelectedSprite();
            }
            if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);
                var nonNewlinesInTheStart = afterCursor.ToCharArray()
                    .Select(c => c).TakeWhile(c => c != '\n').Count();
                CursorPosition = CursorPosition + nonNewlinesInTheStart;

                if (shift)
                {
                    SetSelectedPart(oldCursorPosition);
                }

                ReadjustCursor();
                UpdateSelectedSprite();
            }

            if (!shift)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Left)
                || KeyboardManager.IsUniqueKeyPress(Keys.Right)
                || KeyboardManager.IsUniqueKeyPress(Keys.Up)
                || KeyboardManager.IsUniqueKeyPress(Keys.Down))
                {
                    Selected = false;
                }
            }
        }

        private void SetSelectedPart(int oldCursorPosition)
        {
            if (!Selected)
            {
                Selected = true;
                SelectionBegin = oldCursorPosition;
            }
            var min = Math.Min(SelectionBegin, CursorPosition);
            var max = Math.Max(SelectionBegin, CursorPosition);
            SelectedPart = (min, max);
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
            {
                Selected = true;
                SelectionBegin = 0;
                SelectedPart = (0, RawText.Length);
                CursorPosition = RawText.Length;
                UpdateSelectedSprite();
            }

            // CTRL+C, Copy the text to the clipboard.
            if (KeyboardManager.IsUniqueKeyPress(Keys.C) && Selected)
                Clipboard.SetText(SelectedRawText);

            // CTRL+X, Cut the text to the clipboard.
            if (KeyboardManager.IsUniqueKeyPress(Keys.X) && Selected)
            {
                Clipboard.SetText(SelectedRawText);
                RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                CursorPosition = SelectedPart.start;

                ReadjustTextbox();
                Selected = false;
                UpdateSelectedSprite();
            }

            // CTRL+V Paste text
            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
            {
                var clipboardText = Clipboard.GetText().Replace("\n", "").Replace("\r", "");

                if (!string.IsNullOrEmpty(clipboardText))
                {
                    if (Selected)
                    {
                        var proposed = RawText.Substring(0, SelectedPart.start) + clipboardText +
                                       RawText.Substring(SelectedPart.end, RawText.Length - SelectedPart.end);

                        if (!AllowedCharacters.IsMatch(proposed))
                            return;

                        RawText = proposed;
                        CursorPosition = SelectedPart.start + clipboardText.Length;
                    }
                    else
                    {
                        var proposed = RawText.Substring(0, CursorPosition) + clipboardText +
                                       RawText.Substring(CursorPosition, RawText.Length - CursorPosition);

                        if (!AllowedCharacters.IsMatch(proposed))
                            return;

                        RawText = proposed;
                        CursorPosition += clipboardText.Length;
                    }
                }

                ReadjustTextbox();
                Selected = false;
                UpdateSelectedSprite();
            }

            // CTRL+W or CTRL+Backspace: kill word backwards.
            // This means killing all trailing whitespace and then all trailing non-whitespace.
            if (KeyboardManager.IsUniqueKeyPress(Keys.W) || KeyboardManager.IsUniqueKeyPress(Keys.Back))
            {
                if (Selected)
                {
                    RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                    CursorPosition = SelectedPart.start;
                }
                var upToCursor = RawText.Substring(0, CursorPosition);
                var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);

                var withoutTrailingWhitespace = upToCursor.TrimEnd();
                var nonWhitespacesInTheEnd = withoutTrailingWhitespace.ToCharArray()
                    .Select(c => c).Reverse().TakeWhile(c => !char.IsWhiteSpace(c)).Count();
                RawText = withoutTrailingWhitespace.Substring(0,
                    withoutTrailingWhitespace.Length - nonWhitespacesInTheEnd) + afterCursor;
                    CursorPosition = withoutTrailingWhitespace.Length - nonWhitespacesInTheEnd;

                ReadjustTextbox();
                Selected = false;
                UpdateSelectedSprite();
            }

            // Ctrl+U: kill line backwards.
            // Delete from the cursor position to the start of the line.
            if (KeyboardManager.IsUniqueKeyPress(Keys.U))
            {
                if (Selected)
                {
                    RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                    CursorPosition = SelectedPart.start;
                }
                var upToCursor = RawText.Substring(0, CursorPosition);
                var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);

                var nonNewlinesInTheEnd = upToCursor.ToCharArray()
                    .Select(c => c).Reverse().TakeWhile(c => c != '\n').Count();
                RawText = upToCursor.Substring(0, upToCursor.Length - nonNewlinesInTheEnd) + afterCursor;
                CursorPosition = upToCursor.Length - nonNewlinesInTheEnd;

                ReadjustTextbox();
                Selected = false;
                UpdateSelectedSprite();
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
                CursorPosition = 0;
                Selected = false;
                ReadjustTextbox();
                UpdateSelectedSprite();
            }
        }

        /// <summary>
        ///		Plays a sound sample randomly from the KeyClickSamples list.
        ///	</summary>
        private void PlayKeyClickSound()
        {
            if (KeyClickSamples == null)
                return;

            if(!EnableKeyClickSounds || KeyClickSamples.Count == 0)
                return;

            var r = Rng.Next(KeyClickSamples.Count);
            KeyClickSamples[r].CreateChannel().Play();
        }
    }
}