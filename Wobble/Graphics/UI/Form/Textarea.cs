using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Platform;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Wobble.Graphics.UI.Form
{
    /// <summary>
    ///     A fixed-height text input that soft-wraps and can optionally accept hard newlines.
    /// </summary>
    public class Textarea : Textbox
    {
        private const float HorizontalPadding = 10;
        private const float VerticalPadding = 5;

        private readonly List<Sprite> _selectionSprites = new List<Sprite>();
        private float? _preferredCaretX;
        private float _previousWrapWidth = -1;
        private int? _caretLineAffinity;
        private string _previousLayoutText;

        /// <summary>
        ///     If true, Enter inserts a newline and Ctrl+Enter submits the text.
        /// </summary>
        public bool AllowNewLines { get; set; }

        public Textarea(ScalableVector2 size, WobbleFontStore font, int fontSize,
            string initialText = "", string placeHolderText = "", Action<string> onSubmit = null,
            Action<string> onStoppedTyping = null)
            : base(size, font, fontSize, initialText, placeHolderText, onSubmit, onStoppedTyping)
        {
            InputText.IsCached = true;
            InputText.Alignment = Alignment.TopLeft;
            InputText.X = HorizontalPadding;
            InputText.Y = VerticalPadding;

            Cursor.Alignment = Alignment.TopLeft;
            SelectedSprite.Alignment = Alignment.TopLeft;

            _selectionSprites.Add(SelectedSprite);

            InputEnabled = false;
            AllowMiddleMouseDragging = false;
            AllowScrollbarDragging = false;
            Scrollbar.Width = 5;
            Scrollbar.Visible = false;

            RefreshLayout();
            ChangeCursorLocation();
            UpdateSelectedSprite();
        }

        public override void Update(GameTime gameTime)
        {
            RefreshLayout();
            HandleViewportInput();

            base.Update(gameTime);

            SyncSelectionSpriteStyle();
            AutoScrollWhileSelecting(gameTime);
        }

        protected override string PreparePastedText(string text)
        {
            if (!AllowNewLines)
                return base.PreparePastedText(text);

            return (text ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
        }

        protected override void OnTextInputEntered(object sender, TextInputEventArgs e)
        {
            var oldText = RawText;
            var oldPosition = CursorPosition;
            _caretLineAffinity = null;
            base.OnTextInputEntered(sender, e);

            if (oldText != RawText || oldPosition != CursorPosition)
                _preferredCaretX = null;
        }

        protected override void HandleCtrlInput()
        {
            var oldText = RawText;
            base.HandleCtrlInput();

            if (oldText == RawText)
                return;

            _caretLineAffinity = null;
            _preferredCaretX = null;
            ChangeCursorLocation();
            UpdateSelectedSprite();
        }

        protected override void HandleEnter()
        {
            if (TextInputManager.IsTextCompositionActive)
                return;

            if (TextInputManager.ConsumeTextCompositionCommitPending())
                return;

            if (ConsumeTextInputReceivedThisFrame())
                return;

            if (!Focused)
                return;

            if (!AllowNewLines)
            {
                base.HandleEnter();
                return;
            }

            if (!KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                return;

            if (KeyboardManager.IsCtrlDown())
            {
                if (!AllowSubmission || string.IsNullOrEmpty(RawText))
                    return;

                OnSubmit?.Invoke(RawText);
                _caretLineAffinity = null;
                RawText = "";
                CursorPosition = 0;
                _preferredCaretX = null;
                DeselectAndReadjust();
                return;
            }

            var insertionPosition = Selected ? SelectedPart.start : CursorPosition;
            var selectionEnd = Selected ? SelectedPart.end : CursorPosition;
            var proposed = RawText.Substring(0, insertionPosition) + "\n" +
                           RawText.Substring(selectionEnd);

            if (proposed.Length > MaxCharacters || !AllowedCharacters.IsMatch(proposed))
                return;

            _caretLineAffinity = null;
            RawText = proposed;
            CursorPosition = insertionPosition + 1;
            _preferredCaretX = null;
            PlayKeyClickSound();
            DeselectAndReadjust();
        }

        protected override void HandleArrowKeys(GameTime gameTime)
        {
            if (!Focused)
                return;

            // During IME composition, the native candidate picker owns the navigation keys.
            // Letting them reach the textarea moves its caret behind the composition window.
            if (TextInputManager.IsTextCompositionActive)
                return;

            var shift = KeyboardManager.IsShiftDown();
            var ctrl = KeyboardManager.IsCtrlDown();

            if (IsKeyTriggered(Keys.Left, gameTime))
            {
                if (!TryMoveAcrossSoftWrap(true, shift))
                    MoveCursor(ctrl, true, shift);
                _preferredCaretX = null;
            }

            if (IsKeyTriggered(Keys.Right, gameTime))
            {
                if (!TryMoveAcrossSoftWrap(false, shift))
                    MoveCursor(ctrl, false, shift);
                _preferredCaretX = null;
            }

            if (IsKeyTriggered(Keys.Up, gameTime))
                MoveVertically(-1, shift);

            if (IsKeyTriggered(Keys.Down, gameTime))
                MoveVertically(1, shift);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Home))
            {
                var target = ctrl ? 0 : GetCurrentLine().Start;
                MoveOrSelect(target, shift);
                _preferredCaretX = null;
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.End))
            {
                var target = ctrl ? RawText.Length : GetCurrentLine().End;
                MoveOrSelect(target, shift);
                _preferredCaretX = null;
            }
        }

        protected override (int caretPosition, int textElementStart) GetMouseTextPosition()
        {
            var lines = GetLines();
            if (lines.Count == 0 || string.IsNullOrEmpty(RawText))
                return (0, 0);

            var scaleX = Math.Abs(InputText.AbsoluteScale.X);
            var scaleY = Math.Abs(InputText.AbsoluteScale.Y);
            if (scaleX <= float.Epsilon)
                scaleX = 1;
            if (scaleY <= float.Epsilon)
                scaleY = 1;

            var mouseX = (MouseManager.CurrentState.X - InputText.AbsolutePosition.X) / scaleX;
            var mouseY = (MouseManager.CurrentState.Y - InputText.AbsolutePosition.Y) / scaleY;
            var lineIndex = MathHelper.Clamp((int)Math.Floor(mouseY / GetLineHeight()), 0, lines.Count - 1);
            var line = lines[lineIndex];
            var caret = FindPositionOnLine(line, mouseX);
            _caretLineAffinity = lineIndex;
            var boundaries = GetTextElementBoundaries();
            var boundaryIndex = Array.BinarySearch(boundaries, caret);

            if (boundaryIndex < 0)
                boundaryIndex = Math.Max(0, ~boundaryIndex - 1);
            if (boundaryIndex >= boundaries.Length - 1)
                boundaryIndex = Math.Max(0, boundaries.Length - 2);

            return (caret, boundaries[boundaryIndex]);
        }

        protected override void CalculateContainerX()
        {
            ContentContainer.X = 0;
            ContentContainer.Width = Width;
            ContentContainer.Height = Math.Max(Height, InputText.Y + InputText.Height + VerticalPadding);
        }

        protected override void ChangeCursorLocation()
        {
            if (Cursor == null || InputText == null)
                return;

            var lines = GetLines();
            if (lines.Count == 0)
                return;

            var lineIndex = GetLineIndex(CursorPosition, lines, _caretLineAffinity);
            _caretLineAffinity = lineIndex;
            var line = lines[lineIndex];
            var positionOnLine = Math.Max(line.Start, Math.Min(CursorPosition, line.End));

            Cursor.X = InputText.X + MeasureLineRange(line.Start, positionOnLine);
            Cursor.Y = InputText.Y + lineIndex * GetLineHeight();
            Cursor.Height = GetLineHeight();

            EnsureCaretVisible();
        }

        protected override void UpdateSelectedSprite()
        {
            if (_selectionSprites == null || _selectionSprites.Count == 0)
            {
                base.UpdateSelectedSprite();
                return;
            }

            foreach (var sprite in _selectionSprites)
                sprite.Visible = false;

            if (!Selected)
            {
                SelectedPart = (0, 0);
                return;
            }

            var lines = GetLines();
            var spriteIndex = 0;

            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                var start = Math.Max(SelectedPart.start, line.Start);
                var end = Math.Min(SelectedPart.end, line.End);
                var includesBreak = line.BreakLength > 0 && SelectedPart.start < line.NextStart &&
                                    SelectedPart.end > line.End;

                if (end <= start && !includesBreak)
                    continue;

                var sprite = GetSelectionSprite(spriteIndex++);
                var startX = MeasureLineRange(line.Start, Math.Min(start, line.End));
                var endX = MeasureLineRange(line.Start, Math.Max(start, end));

                sprite.Alignment = Alignment.TopLeft;
                sprite.X = InputText.X + startX;
                sprite.Y = InputText.Y + lineIndex * GetLineHeight();
                sprite.Width = Math.Max(includesBreak ? Cursor.Width : 0, endX - startX);
                sprite.Height = GetLineHeight();
                sprite.Visible = true;
            }
        }

        private void RefreshLayout()
        {
            var wrapWidth = Math.Max(1, Width - HorizontalPadding * 2);
            var textChanged = _previousLayoutText != RawText;
            if (textChanged)
            {
                _previousLayoutText = RawText;
                _caretLineAffinity = null;
            }

            if (textChanged || Math.Abs(_previousWrapWidth - wrapWidth) > float.Epsilon)
            {
                _previousWrapWidth = wrapWidth;
                InputText.MaxWidth = wrapWidth;
                ChangeCursorLocation();
                UpdateSelectedSprite();
            }

            Button.Size = Size;
            CalculateContainerX();
            Scrollbar.Visible = ContentContainer.Height > Height;
        }

        private List<WrappedTextLine> GetLines() => InputText.BuildWrappedLayout(RawText);

        private WrappedTextLine GetCurrentLine()
        {
            var lines = GetLines();
            return lines[GetLineIndex(CursorPosition, lines, _caretLineAffinity)];
        }

        private static int GetLineIndex(int position, IReadOnlyList<WrappedTextLine> lines, int? affinity = null)
        {
            if (affinity >= 0 && affinity < lines.Count &&
                position >= lines[affinity.Value].Start && position <= lines[affinity.Value].End)
                return affinity.Value;

            for (var i = lines.Count - 1; i >= 0; i--)
            {
                if (position >= lines[i].Start && position <= lines[i].End)
                    return i;
            }

            for (var i = 0; i < lines.Count; i++)
            {
                if (position < lines[i].NextStart)
                    return i;
            }

            return Math.Max(0, lines.Count - 1);
        }

        private float GetLineHeight()
        {
            var line = InputText.Children.OfType<SpriteTextPlusLine>().FirstOrDefault();
            return line?.LayoutHeight ?? Math.Max(1, Cursor?.Height ?? InputText.Height);
        }

        private float MeasureLineRange(int start, int end)
        {
            if (end <= start)
                return 0;

            return InputText.MeasureLayoutLineWidth(RawText.Substring(start, end - start));
        }

        private int FindPositionOnLine(WrappedTextLine line, float targetX)
        {
            if (targetX <= 0 || line.Length == 0)
                return line.Start;

            var boundaries = GetTextElementBoundaries()
                .Where(x => x >= line.Start && x <= line.End)
                .ToArray();

            if (boundaries.Length == 0)
                return line.Start;

            if (targetX >= MeasureLineRange(line.Start, line.End))
                return line.End;

            var low = 1;
            var high = boundaries.Length - 1;

            while (low < high)
            {
                var middle = low + (high - low) / 2;
                if (MeasureLineRange(line.Start, boundaries[middle]) < targetX)
                    low = middle + 1;
                else
                    high = middle;
            }

            var right = boundaries[low];
            var left = boundaries[low - 1];
            var leftWidth = MeasureLineRange(line.Start, left);
            var rightWidth = MeasureLineRange(line.Start, right);
            return targetX - leftWidth <= rightWidth - targetX ? left : right;
        }

        private void MoveVertically(int direction, bool select)
        {
            var lines = GetLines();
            var currentIndex = GetLineIndex(CursorPosition, lines, _caretLineAffinity);
            var targetIndex = MathHelper.Clamp(currentIndex + direction, 0, lines.Count - 1);

            if (targetIndex == currentIndex)
                return;

            if (_preferredCaretX == null)
            {
                var current = lines[currentIndex];
                _preferredCaretX = MeasureLineRange(current.Start,
                    Math.Max(current.Start, Math.Min(CursorPosition, current.End)));
            }

            _caretLineAffinity = targetIndex;
            MoveOrSelect(FindPositionOnLine(lines[targetIndex], _preferredCaretX.Value), select);
        }

        private void MoveOrSelect(int target, bool select)
        {
            if (select)
                SetSelectionFromAnchor(Selected ? SelectionBegin : CursorPosition, target);
            else
                MoveCaretTo(target);
        }

        private bool TryMoveAcrossSoftWrap(bool left, bool select)
        {
            if (select || Selected || KeyboardManager.IsCtrlDown())
                return false;

            var lines = GetLines();
            var currentIndex = GetLineIndex(CursorPosition, lines, _caretLineAffinity);

            if (left && currentIndex > 0 && CursorPosition == lines[currentIndex].Start &&
                lines[currentIndex - 1].End == CursorPosition && lines[currentIndex - 1].BreakLength == 0)
            {
                _caretLineAffinity = currentIndex - 1;
                MoveCaretTo(CursorPosition);
                return true;
            }

            if (!left && currentIndex < lines.Count - 1 && CursorPosition == lines[currentIndex].End &&
                lines[currentIndex + 1].Start == CursorPosition && lines[currentIndex].BreakLength == 0)
            {
                _caretLineAffinity = currentIndex + 1;
                MoveCaretTo(CursorPosition);
                return true;
            }

            return false;
        }

        private bool IsKeyTriggered(Keys key, GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(key))
            {
                LastCursorMove = gameTime.TotalGameTime.TotalMilliseconds;
                return true;
            }

            if (!KeyHeldFor.ContainsKey(key) || KeyHeldFor[key] <= 750 ||
                gameTime.TotalGameTime.TotalMilliseconds - LastCursorMove <= 75)
                return false;

            LastCursorMove = gameTime.TotalGameTime.TotalMilliseconds;
            return true;
        }

        private Sprite GetSelectionSprite(int index)
        {
            while (_selectionSprites.Count <= index)
            {
                var sprite = new Sprite
                {
                    Alignment = Alignment.TopLeft,
                    Tint = SelectedSprite.Tint,
                    Alpha = SelectedSprite.Alpha
                };
                _selectionSprites.Add(sprite);
                AddContainedDrawable(sprite);
            }

            return _selectionSprites[index];
        }

        private void SyncSelectionSpriteStyle()
        {
            for (var i = 1; i < _selectionSprites.Count; i++)
            {
                _selectionSprites[i].Tint = SelectedSprite.Tint;
                _selectionSprites[i].Alpha = SelectedSprite.Alpha;
            }
        }

        private void EnsureCaretVisible()
        {
            if (Cursor == null || ContentContainer == null)
                return;

            var top = Cursor.Y + ContentContainer.Y;
            var bottom = top + Cursor.Height;

            if (top < VerticalPadding)
                TargetY += VerticalPadding - top;
            else if (bottom > Height - VerticalPadding)
                TargetY -= bottom - (Height - VerticalPadding);
        }

        private void HandleViewportInput()
        {
            if (IsHovered())
            {
                if (MouseManager.IsScrollingUp(InvertedScrolling))
                    TargetY += ScrollSpeed;
                else if (MouseManager.IsScrollingDown(InvertedScrolling))
                    TargetY -= ScrollSpeed;
            }

            if (!Focused)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.PageUp))
                TargetY += ScrollSpeed * 5;
            else if (KeyboardManager.IsUniqueKeyPress(Keys.PageDown))
                TargetY -= ScrollSpeed * 5;
        }

        private void AutoScrollWhileSelecting(GameTime gameTime)
        {
            if (!Button.IsHeld || !MouseManager.IsPressed(MouseButton.Left))
                return;

            var distance = 0f;
            if (MouseManager.CurrentState.Y < ScreenRectangle.Top)
                distance = MouseManager.CurrentState.Y - ScreenRectangle.Top;
            else if (MouseManager.CurrentState.Y > ScreenRectangle.Bottom)
                distance = MouseManager.CurrentState.Y - ScreenRectangle.Bottom;

            if (Math.Abs(distance) <= float.Epsilon)
                return;

            TargetY -= distance * (float)gameTime.ElapsedGameTime.TotalSeconds * 8;
        }
    }
}
