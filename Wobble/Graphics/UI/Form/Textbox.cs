using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const double MultiClickThreshold = 500;
        private const float MultiClickDistance = 5;

        private enum MouseSelectionMode
        {
            Character,
            Word,
            All
        }

        private enum TextRunKind
        {
            Word,
            Whitespace,
            Other
        }

        private sealed class TextboxInteractionButton : ImageButton
        {
            private readonly Action<GameTime> _onPressed;
            private readonly Action<GameTime> _onHeld;
            private readonly Action _onReleased;

            public TextboxInteractionButton(Action<GameTime> onPressed, Action<GameTime> onHeld,
                Action onReleased, EventHandler clickAction)
                : base(WobbleAssets.WhiteBox, clickAction)
            {
                _onPressed = onPressed;
                _onHeld = onHeld;
                _onReleased = onReleased;
            }

            public override void Update(GameTime gameTime)
            {
                var wasHeld = IsHeld;

                base.Update(gameTime);

                if (IsHeld && MouseManager.IsUniquePress(MouseButton.Left))
                    _onPressed?.Invoke(gameTime);

                if (IsHeld && MouseManager.IsPressed(MouseButton.Left))
                    _onHeld?.Invoke(gameTime);

                if (wasHeld && !IsHeld)
                    _onReleased?.Invoke();
            }
        }

        /// <summary>
        ///     The text that is currently displayed
        /// </summary>
        public SpriteTextPlus InputText { get; }

        /// <summary>
        ///     The cursor that displays where the text input currently is.
        /// </summary>
        public Sprite Cursor { get; }

        /// <summary>
        ///     Displays the currently selected range of text.
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
                TextElementBoundaryText = null;

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

                if (!AllowCursorMovement)
                    CursorPosition = RawText.Length;
                else
                    CursorPosition = Math.Min(CursorPosition, RawText.Length);
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
        ///     Determines if any text is selected.
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

        private string TextElementBoundaryText { get; set; }

        private int[] TextElementBoundaries { get; set; } = new[] { 0 };

        private bool IsMouseSelecting { get; set; }

        private MouseSelectionMode CurrentMouseSelectionMode { get; set; }

        private int MouseSelectionAnchor { get; set; }

        private (int start, int end) MouseWordSelection { get; set; }

        private int MouseClickCount { get; set; }

        private double LastMousePressTime { get; set; } = double.NegativeInfinity;

        private Vector2 LastMousePressPosition { get; set; }

        /// <summary>
        ///    The position of the cursor in the textbox. In amount of characters from the start.
        /// </summary>
        private int _cursorPosition = 0;
        public int CursorPosition
        {
            get => _cursorPosition;
            private set
            {
                _cursorPosition = GetNearestTextElementBoundary(value);
                ChangeCursorLocation();
            }
        }

        /// <summary>
        ///    If true, it'll allow the cursor to move around using the arrow keys.
        /// </summary>
        public bool AllowCursorMovement { get; set; } = true;

        /// <summary>
        ///    The time since the cursor has last moved.
        /// </summary>
        private double lastCursorMove { get; set; } = 0;

        /// <summary>
        ///    The keys that are currently being held down, and for how long.
        /// </summary>
        private Dictionary<Keys, double> keyHeldFor { get; set; } = new Dictionary<Keys, double>();

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
            string initialText = "", string placeHolderText = "", Action<string> onSubmit = null, Action<string> onStoppedTyping = null)
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
            Button = new TextboxInteractionButton(HandleMousePressed, HandleMouseHeld, HandleMouseReleased,
                (o, e) => Focused = true)
            {
                Parent = this,
                Size = Size,
                Alpha = 0
            };

            // If the user clicks outside of the button, then it won't be focused anymore.
            Button.ClickedOutside += (o, e) =>
            {
                if (!IsMouseSelecting)
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

            UpdateKeyHeldFor(gameTime);

            // Handle all input.
            if (AllowCursorMovement)
                HandleArrowKeys(gameTime);
            HandleCtrlInput();
            HandleEnter();
            CalculateContainerX();

            // Change the alpha of the selected sprite depending on whether text is selected.
            SelectedSprite.Alpha = MathHelper.Lerp(SelectedSprite.Alpha, Selected ? 0.5f : 0,
                (float)Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

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

        private int[] GetTextElementBoundaries()
        {
            var text = RawText ?? "";

            if (TextElementBoundaryText == text)
                return TextElementBoundaries;

            var starts = StringInfo.ParseCombiningCharacters(text);
            TextElementBoundaries = new int[starts.Length + 1];
            Array.Copy(starts, TextElementBoundaries, starts.Length);
            TextElementBoundaries[TextElementBoundaries.Length - 1] = text.Length;
            TextElementBoundaryText = text;

            return TextElementBoundaries;
        }

        private int GetNearestTextElementBoundary(int position)
        {
            var textLength = RawText?.Length ?? 0;
            position = Math.Max(0, Math.Min(position, textLength));

            var boundaries = GetTextElementBoundaries();
            var index = Array.BinarySearch(boundaries, position);

            if (index >= 0)
                return boundaries[index];

            var nextIndex = ~index;
            var previousIndex = Math.Max(0, nextIndex - 1);

            if (nextIndex >= boundaries.Length)
                return boundaries[previousIndex];

            return position - boundaries[previousIndex] <= boundaries[nextIndex] - position
                ? boundaries[previousIndex]
                : boundaries[nextIndex];
        }

        private int GetBoundaryIndex(int position)
        {
            var boundaries = GetTextElementBoundaries();
            position = GetNearestTextElementBoundary(position);
            return Array.BinarySearch(boundaries, position);
        }

        private int GetPreviousTextElementBoundary(int position)
        {
            var boundaries = GetTextElementBoundaries();
            var index = GetBoundaryIndex(position);
            return boundaries[Math.Max(0, index - 1)];
        }

        private int GetNextTextElementBoundary(int position)
        {
            var boundaries = GetTextElementBoundaries();
            var index = GetBoundaryIndex(position);
            return boundaries[Math.Min(boundaries.Length - 1, index + 1)];
        }

        private float MeasureTextWidth(int end)
        {
            if (end <= 0)
                return 0;

            InputText.Font.FontSize = InputText.FontSize;
            return InputText.Font.Store.MeasureString(RawText.Substring(0, end)).X;
        }

        private (int caretPosition, int textElementStart) GetMouseTextPosition()
        {
            var boundaries = GetTextElementBoundaries();

            if (boundaries.Length == 1)
                return (0, 0);

            var absoluteScale = Math.Abs(InputText.AbsoluteScale.X);
            if (absoluteScale <= float.Epsilon)
                absoluteScale = 1;

            var mouseX = (MouseManager.CurrentState.X - InputText.AbsolutePosition.X) / absoluteScale;

            if (mouseX <= 0)
                return (0, boundaries[0]);

            var totalWidth = MeasureTextWidth(RawText.Length);
            if (mouseX >= totalWidth)
                return (RawText.Length, boundaries[boundaries.Length - 2]);

            var low = 1;
            var high = boundaries.Length - 1;

            while (low < high)
            {
                var middle = low + (high - low) / 2;

                if (MeasureTextWidth(boundaries[middle]) < mouseX)
                    low = middle + 1;
                else
                    high = middle;
            }

            var rightBoundary = boundaries[low];
            var leftBoundary = boundaries[low - 1];
            var leftWidth = MeasureTextWidth(leftBoundary);
            var rightWidth = MeasureTextWidth(rightBoundary);
            var caretPosition = mouseX - leftWidth <= rightWidth - mouseX
                ? leftBoundary
                : rightBoundary;

            return (caretPosition, leftBoundary);
        }

        private TextRunKind GetTextRunKind(int textElementStart)
        {
            if (char.IsWhiteSpace(RawText, textElementStart))
                return TextRunKind.Whitespace;

            switch (CharUnicodeInfo.GetUnicodeCategory(RawText, textElementStart))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.EnclosingMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.OtherNumber:
                case UnicodeCategory.ConnectorPunctuation:
                    return TextRunKind.Word;
                default:
                    return TextRunKind.Other;
            }
        }

        private (int start, int end) GetTextRun(int textElementStart)
        {
            if (string.IsNullOrEmpty(RawText))
                return (0, 0);

            var boundaries = GetTextElementBoundaries();
            var boundaryIndex = Array.BinarySearch(boundaries, textElementStart);

            if (boundaryIndex < 0)
                boundaryIndex = Math.Max(0, ~boundaryIndex - 1);

            boundaryIndex = Math.Min(boundaryIndex, boundaries.Length - 2);
            var kind = GetTextRunKind(boundaries[boundaryIndex]);
            var startIndex = boundaryIndex;
            var endIndex = boundaryIndex + 1;

            while (startIndex > 0 && GetTextRunKind(boundaries[startIndex - 1]) == kind)
                startIndex--;

            while (endIndex < boundaries.Length - 1 && GetTextRunKind(boundaries[endIndex]) == kind)
                endIndex++;

            return (boundaries[startIndex], boundaries[endIndex]);
        }

        private int GetPreviousWordBoundary(int position)
        {
            var boundaries = GetTextElementBoundaries();
            var index = GetBoundaryIndex(position);

            while (index > 0 && GetTextRunKind(boundaries[index - 1]) == TextRunKind.Whitespace)
                index--;

            if (index == 0)
                return 0;

            var kind = GetTextRunKind(boundaries[index - 1]);
            while (index > 0 && GetTextRunKind(boundaries[index - 1]) == kind)
                index--;

            return boundaries[index];
        }

        private int GetNextWordBoundary(int position)
        {
            var boundaries = GetTextElementBoundaries();
            var index = GetBoundaryIndex(position);

            if (index >= boundaries.Length - 1)
                return RawText.Length;

            var kind = GetTextRunKind(boundaries[index]);

            if (kind == TextRunKind.Whitespace)
            {
                while (index < boundaries.Length - 1 &&
                       GetTextRunKind(boundaries[index]) == TextRunKind.Whitespace)
                    index++;
            }
            else
            {
                while (index < boundaries.Length - 1 && GetTextRunKind(boundaries[index]) == kind)
                    index++;

                while (index < boundaries.Length - 1 &&
                       GetTextRunKind(boundaries[index]) == TextRunKind.Whitespace)
                    index++;
            }

            return boundaries[index];
        }

        private int GetNextWordDeletionBoundary(int position)
        {
            var boundaries = GetTextElementBoundaries();
            var index = GetBoundaryIndex(position);

            while (index < boundaries.Length - 1 &&
                   GetTextRunKind(boundaries[index]) == TextRunKind.Whitespace)
                index++;

            if (index < boundaries.Length - 1)
            {
                var kind = GetTextRunKind(boundaries[index]);
                while (index < boundaries.Length - 1 && GetTextRunKind(boundaries[index]) == kind)
                    index++;
            }

            return boundaries[index];
        }

        private void MoveCaretTo(int position)
        {
            CursorPosition = position;
            SelectionBegin = CursorPosition;
            Selected = false;
            SelectedPart = (0, 0);
            UpdateSelectedSprite();
            ReadjustCursor();
            CalculateContainerX();
        }

        private void SetSelectionFromAnchor(int anchor, int caret)
        {
            SelectionBegin = GetNearestTextElementBoundary(anchor);
            CursorPosition = caret;

            var min = Math.Min(SelectionBegin, CursorPosition);
            var max = Math.Max(SelectionBegin, CursorPosition);
            Selected = min != max;
            SelectedPart = Selected ? (min, max) : (0, 0);

            UpdateSelectedSprite();
            ReadjustCursor();
            CalculateContainerX();
        }

        private void HandleMousePressed(GameTime gameTime)
        {
            Focused = true;
            ReadjustCursor();

            if (!AllowCursorMovement)
                return;

            var mousePosition = MouseManager.CurrentState.Position;
            var withinClickTime =
                gameTime.TotalGameTime.TotalMilliseconds - LastMousePressTime <= MultiClickThreshold;
            var withinClickDistance =
                Vector2.DistanceSquared(mousePosition, LastMousePressPosition) <= MultiClickDistance * MultiClickDistance;

            MouseClickCount = withinClickTime && withinClickDistance
                ? MouseClickCount % 3 + 1
                : 1;
            LastMousePressTime = gameTime.TotalGameTime.TotalMilliseconds;
            LastMousePressPosition = mousePosition;

            var hit = GetMouseTextPosition();
            IsMouseSelecting = true;

            if (KeyboardManager.IsShiftDown())
            {
                MouseClickCount = 0;
                CurrentMouseSelectionMode = MouseSelectionMode.Character;
                MouseSelectionAnchor = Selected ? SelectionBegin : CursorPosition;
                SetSelectionFromAnchor(MouseSelectionAnchor, hit.caretPosition);
                return;
            }

            switch (MouseClickCount)
            {
                case 1:
                    CurrentMouseSelectionMode = MouseSelectionMode.Character;
                    MouseSelectionAnchor = hit.caretPosition;
                    MoveCaretTo(hit.caretPosition);
                    break;
                case 2:
                    CurrentMouseSelectionMode = MouseSelectionMode.Word;
                    MouseWordSelection = GetTextRun(hit.textElementStart);
                    SetSelectionFromAnchor(MouseWordSelection.start, MouseWordSelection.end);
                    break;
                default:
                    CurrentMouseSelectionMode = MouseSelectionMode.All;
                    SetSelectionFromAnchor(0, RawText.Length);
                    break;
            }
        }

        private void HandleMouseHeld(GameTime gameTime)
        {
            if (!AllowCursorMovement || !IsMouseSelecting)
                return;

            var hit = GetMouseTextPosition();

            switch (CurrentMouseSelectionMode)
            {
                case MouseSelectionMode.Character:
                    SetSelectionFromAnchor(MouseSelectionAnchor, hit.caretPosition);
                    break;
                case MouseSelectionMode.Word:
                    var currentWord = GetTextRun(hit.textElementStart);

                    if (currentWord.start < MouseWordSelection.start)
                        SetSelectionFromAnchor(MouseWordSelection.end, currentWord.start);
                    else if (currentWord.end > MouseWordSelection.end)
                        SetSelectionFromAnchor(MouseWordSelection.start, currentWord.end);
                    else
                        SetSelectionFromAnchor(MouseWordSelection.start, MouseWordSelection.end);
                    break;
                case MouseSelectionMode.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleMouseReleased()
        {
            if (Vector2.DistanceSquared(MouseManager.CurrentState.Position, LastMousePressPosition) >
                MultiClickDistance * MultiClickDistance)
                MouseClickCount = 0;

            IsMouseSelecting = false;
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
            if (KeyboardManager.IsCtrlDown())
                return;

            // Enter is handled in Update() because TextInput only receives the regular Enter and not the NumPad Enter.
            if (e.Key == Keys.Enter)
                return;

            // If the text is selected
            if (Selected)
            {
                switch (e.Key)
                {
                    case Keys.Tab:
                    case Keys.Escape:
                    case Keys.VolumeUp:
                    case Keys.VolumeDown:
                        return;
                    case Keys.Back:
                    case Keys.Delete:
                        RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                        CursorPosition = SelectedPart.start;
                        PlayKeyClickSound();
                        break;
                    default:
                        var upToCursor = RawText.Substring(0, SelectedPart.start);
                        var afterCursor = RawText.Substring(SelectedPart.end, RawText.Length - SelectedPart.end);

                        upToCursor += e.Character;
                        var proposedText = upToCursor + afterCursor;

                        if (proposedText.Length > MaxCharacters || !AllowedCharacters.IsMatch(proposedText))
                            return;

                        RawText = proposedText;
                        CursorPosition = upToCursor.Length;
                        PlayKeyClickSound();
                        break;
                }

                SelectionBegin = CursorPosition;
                Selected = false;
                SelectedPart = (0, 0);
                UpdateSelectedSprite();
            }
            // Handle normal key presses.
            else
            {
                var upToCursor = RawText.Substring(0, CursorPosition);
                var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);
                // Handle key inputs.
                switch (e.Key)
                {
                    // Ignore these keys
                    case Keys.Tab:
                    case Keys.Escape:
                    case Keys.VolumeUp:
                    case Keys.VolumeDown:
                        return;
                    // text deletion
                    case Keys.Back:
                        if (CursorPosition == 0)
                            return;

                        var previousBoundary = GetPreviousTextElementBoundary(CursorPosition);
                        RawText = RawText.Remove(previousBoundary, CursorPosition - previousBoundary);
                        CursorPosition = previousBoundary;
                        PlayKeyClickSound();
                        break;
                    case Keys.Delete:
                        if (CursorPosition == RawText.Length)
                            return;

                        var nextBoundary = GetNextTextElementBoundary(CursorPosition);
                        RawText = RawText.Remove(CursorPosition, nextBoundary - CursorPosition);
                        PlayKeyClickSound();
                        break;
                    // Input text
                    default:
                        if (RawText.Length + 1 <= MaxCharacters)
                        {
                            upToCursor += e.Character;

                            var proposedText = upToCursor + afterCursor;

                            if (!AllowedCharacters.IsMatch(proposedText))
                                return;

                            RawText = proposedText;
                            CursorPosition = upToCursor.Length;

                            PlayKeyClickSound();
                        }
                        break;
                }
            }

            ReadjustTextbox();
        }

        /// <summary>
        ///    If it's a single lined textbox, then we need to move the ContentContainer (Viewinew container),
        ///     either to the left or to the right depending on where the cursor is.
        /// </summary>
        private void CalculateContainerX()
        {
            ContentContainer.Width = InputText.Width;

            if (!AllowCursorMovement)
            {
                ContentContainer.X = InputText.Width + 20 > Width ? Width - InputText.Width - Cursor.Width - 20 : 0;
                return;
            }

            if (InputText.Width + 20 <= Width || string.IsNullOrEmpty(RawText))
            {
                ContentContainer.X = 0;
                return;
            }

            var absOffsetFromLeft = Cursor.AbsolutePosition.X - AbsolutePosition.X;
            var absOffsetFromRight = (Cursor.AbsolutePosition.X + Cursor.AbsoluteSize.X) - (AbsolutePosition.X + AbsoluteSize.X);
            var offsetFromLeft = Cursor.X - X;
            var offsetFromRight = (Cursor.X + Cursor.Width) - (X + Width);

            if (absOffsetFromLeft < 5)
            {
                ContentContainer.X = Math.Min(0, -offsetFromLeft + 5);
            }
            else if (absOffsetFromRight > -5)
            {
                ContentContainer.X = -offsetFromRight - 5;
            }
        }

        /// <summary>
        ///     Changes the location of the cursor to the position of where the text is.
        /// </summary>
        private void ChangeCursorLocation()
        {
            if (Cursor == null || InputText == null)
                return;
            if (!AllowCursorMovement || string.IsNullOrEmpty(RawText))
            {
                Cursor.X = string.IsNullOrEmpty(RawText) ? InputText.X : InputText.X + InputText.Width;
                return;
            }

            var substring = RawText.Substring(0, CursorPosition);
            InputText.Font.FontSize = InputText.FontSize;
            var x = InputText.Font.Store.MeasureString(substring).X;

            Cursor.X = x + InputText.X;
        }

        /// <summary>
        ///    Updates the selected sprite to match the selected text.
        /// </summary>
        private void UpdateSelectedSprite()
        {
            if (SelectedSprite == null)
                return;

            if (!AllowCursorMovement)
            {
                SelectedSprite.Visible = Selected;
                SelectedSprite.Width = InputText.Width;
                SelectedSprite.X = InputText.X;
                return;
            }
            SelectedSprite.Visible = Selected;
            if (!Selected)
            {
                SelectedPart = (0, 0);
            }
            var x = MeasureTextWidth(SelectedPart.start);
            var width = MeasureTextWidth(SelectedPart.end) - x;

            SelectedSprite.X = x + InputText.X;
            SelectedSprite.Width = width;
        }

        /// <summary>
        ///    Updates the keyHeldFor dictionary with the keys that are currently being held down.
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateKeyHeldFor(GameTime gameTime)
        {
            if (!Focused)
            {
                keyHeldFor.Clear();
                return;
            }
            var keys = KeyboardManager.CurrentState.GetPressedKeys();
            foreach (var key in keys)
            {
                if (!keyHeldFor.ContainsKey(key))
                    keyHeldFor.Add(key, 0);
                else
                    keyHeldFor[key] += gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            foreach (var key in keyHeldFor.Keys.ToList())
            {
                if (!keys.Contains(key))
                    keyHeldFor.Remove(key);
            }
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
        ///    Makes sure the cursor is visible and resets its visiblity changing.
        /// </summary>
        public void ReadjustCursor()
        {
            Cursor.Visible = true;
            TimeSinceCursorVisibllityChanged = 0;
        }

        /// <summary>
        ///    Handles the arrow keys for the textbox.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleArrowKeys(GameTime gameTime)
        {
            if (!Focused)
                return;

            var shift = KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift);
            var ctrl = KeyboardManager.IsCtrlDown();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left)
            || (keyHeldFor.ContainsKey(Keys.Left) && keyHeldFor[Keys.Left] > 750
                && gameTime.TotalGameTime.TotalMilliseconds - lastCursorMove > 75))
            {
                MoveCursor(ctrl, true, shift);
                lastCursorMove = gameTime.TotalGameTime.TotalMilliseconds;
            }
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right)
            || (keyHeldFor.ContainsKey(Keys.Right) && keyHeldFor[Keys.Right] > 750
                && gameTime.TotalGameTime.TotalMilliseconds - lastCursorMove > 75))
            {
                MoveCursor(ctrl, false, shift);
                lastCursorMove = gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Home))
            {
                if (shift)
                    SetSelectionFromAnchor(Selected ? SelectionBegin : CursorPosition, 0);
                else
                    MoveCaretTo(0);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.End))
            {
                if (shift)
                    SetSelectionFromAnchor(Selected ? SelectionBegin : CursorPosition, RawText.Length);
                else
                    MoveCaretTo(RawText.Length);
            }
        }

        /// <summary>
        ///     Moves the cursor to the next/previous character/word.
        /// </summary>
        /// <param name="wholeWord"></param>
        /// <param name="left"></param>
        private void MoveCursor(bool wholeWord, bool left, bool select = false)
        {
            var oldCursorPosition = CursorPosition;

            if (!select && Selected)
            {
                MoveCaretTo(left ? SelectedPart.start : SelectedPart.end);
                return;
            }

            int newCursorPosition;

            if (wholeWord)
                newCursorPosition = left
                    ? GetPreviousWordBoundary(CursorPosition)
                    : GetNextWordBoundary(CursorPosition);
            else if (left)
                newCursorPosition = GetPreviousTextElementBoundary(CursorPosition);
            else
                newCursorPosition = GetNextTextElementBoundary(CursorPosition);

            if (select)
                SetSelectionFromAnchor(Selected ? SelectionBegin : oldCursorPosition, newCursorPosition);
            else
                MoveCaretTo(newCursorPosition);
        }

        /// <summary>
        ///     Deselects the text and readjusts the textbox.
        /// </summary>
        private void DeselectAndReadjust()
        {
            ReadjustTextbox();
            SelectionBegin = CursorPosition;
            Selected = false;
            SelectedPart = (0, 0);
            UpdateSelectedSprite();
        }

        /// <summary>
        ///     Handles control input for the textbox.
        /// </summary>
        private void HandleCtrlInput()
        {
            // Make sure the textbox is focused and that the control buttons are down before handling anything.
            if (!Focused || !KeyboardManager.IsCtrlDown())
                return;

            // CTRL+A, Select the text.
            if (KeyboardManager.IsUniqueKeyPress(Keys.A) && !string.IsNullOrEmpty(RawText))
                SetSelectionFromAnchor(0, RawText.Length);

            // CTRL+C, Copy the text to the clipboard.
            if (KeyboardManager.IsUniqueKeyPress(Keys.C) && Selected)
                Clipboard.SetText(SelectedRawText);

            // CTRL+X, Cut the text to the clipboard.
            if (KeyboardManager.IsUniqueKeyPress(Keys.X) && Selected)
            {
                Clipboard.SetText(SelectedRawText);
                RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                CursorPosition = SelectedPart.start;

                DeselectAndReadjust();
            }

            // CTRL+V Paste text
            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
            {
                var clipboardText = Clipboard.GetText().Replace("\n", "").Replace("\r", "");

                if (!string.IsNullOrEmpty(clipboardText))
                {
                    if (Selected)
                    {
                        var upToCursor = RawText.Substring(0, SelectedPart.start);
                        var afterCursor = RawText.Substring(SelectedPart.end, RawText.Length - SelectedPart.end);

                        upToCursor += clipboardText;

                        var proposed = upToCursor + afterCursor;

                        if (proposed.Length > MaxCharacters || !AllowedCharacters.IsMatch(proposed))
                            return;

                        RawText = proposed;
                        CursorPosition = upToCursor.Length;
                    }
                    else
                    {
                        var upToCursor = RawText.Substring(0, CursorPosition);
                        var afterCursor = RawText.Substring(CursorPosition, RawText.Length - CursorPosition);

                        upToCursor += clipboardText;

                        var proposed = upToCursor + afterCursor;

                        if (proposed.Length > MaxCharacters || !AllowedCharacters.IsMatch(proposed))
                            return;

                        RawText = proposed;
                        CursorPosition = upToCursor.Length;
                    }
                }

                DeselectAndReadjust();
            }

            // CTRL+W or CTRL+Backspace: kill word backwards.
            if (KeyboardManager.IsUniqueKeyPress(Keys.W) || KeyboardManager.IsUniqueKeyPress(Keys.Back))
            {
                if (Selected)
                {
                    RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                    CursorPosition = SelectedPart.start;
                }
                else
                {
                    var previousBoundary = GetPreviousWordBoundary(CursorPosition);
                    RawText = RawText.Remove(previousBoundary, CursorPosition - previousBoundary);
                    CursorPosition = previousBoundary;
                }

                DeselectAndReadjust();
            }

            // CTRL+DELETE: kill word forwards.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
            {
                if (Selected)
                {
                    RawText = RawText.Remove(SelectedPart.start, SelectedPart.end - SelectedPart.start);
                    CursorPosition = SelectedPart.start;
                }
                else
                {
                    var nextBoundary = GetNextWordDeletionBoundary(CursorPosition);
                    RawText = RawText.Remove(CursorPosition, nextBoundary - CursorPosition);
                }

                DeselectAndReadjust();
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

                DeselectAndReadjust();
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
                DeselectAndReadjust();
            }
        }

        /// <summary>
        ///		Plays a sound sample randomly from the KeyClickSamples list.
        ///	</summary>
        private void PlayKeyClickSound()
        {
            if (KeyClickSamples == null)
                return;

            if (!EnableKeyClickSounds || KeyClickSamples.Count == 0)
                return;

            var r = Rng.Next(KeyClickSamples.Count);
            KeyClickSamples[r].CreateChannel().Play();
        }
    }
}
