using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Wobble.Graphics.UI.Form
{
    public class HorizontalSelector : Sprite
    {
        /// <summary>
        ///     The list of options that are contained in the horizontal selector.
        /// </summary>
        public List<string> Options { get; }

        /// <summary>
        ///     The index of the currently selected element.
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>
        ///     When the horizontal selector's value changes, this method will be called.
        ///
        ///     Parameters:
        ///         string: The newly selected option
        ///         int: The index of the newly selected option
        /// </summary>
        private Action<string, int> OnChange { get; }

        /// <summary>
        ///     The text that displays the
        /// </summary>
        public SpriteText SelectedItemText { get; }

        /// <summary>
        ///     The button to select the option to the left.
        /// </summary>
        public ImageButton ButtonSelectLeft { get; }

        /// <summary>
        ///     The button to select the option to the right.
        /// </summary>
        public ImageButton ButtonSelectRight { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="selectorSize"></param>
        /// <param name="selectorFont"></param>
        /// <param name="fontScale"></param>
        /// <param name="leftButtonImage"></param>
        /// <param name="rightButtonImage"></param>
        /// <param name="buttonSize"></param>
        /// <param name="buttonSpacing"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        public HorizontalSelector(List<string> options, ScalableVector2 selectorSize, string selectorFont, int fontSize, Texture2D leftButtonImage,
                                    Texture2D rightButtonImage, ScalableVector2 buttonSize, int buttonSpacing, Action<string, int> onChange,
                                    int selectedIndex = 0)
        {
            if (options.Count == 0)
                throw new ArgumentException("HorizontalSelector must be initialized with more than one option.");

            Options = options;
            OnChange = onChange;

            if (SelectedIndex < 0 || selectedIndex > Options.Count - 1)
                throw new ArgumentException("Default selectedIndex must be in range of the options.");

            SelectedIndex = selectedIndex;
            Size = selectorSize;

            // Create the text that displays the currently selected item.
            SelectedItemText = new SpriteText(selectorFont, Options[SelectedIndex], fontSize)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black
            };

            // Create the left selection button.
            ButtonSelectLeft = new ImageButton(leftButtonImage, (sender, e) => HandleSelection(Direction.Backward))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = buttonSize,
                Image = leftButtonImage,
                X = -buttonSize.X.Value - buttonSpacing
            };

            // Create the right selection button.
            ButtonSelectRight = new ImageButton(rightButtonImage, (sender, e) => HandleSelection(Direction.Forward))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = buttonSize,
                Image = rightButtonImage,
                X = buttonSize.X.Value + buttonSpacing
            };
        }

        /// <summary>
        ///     Handles the selection of the next element based on the direction.
        /// </summary>
        /// <param name="direction"></param>
        private void HandleSelection(Direction direction)
        {
            // Choose the newly selected index based on the direction we're going.
            switch (direction)
            {
                case Direction.Backward:
                    if (SelectedIndex - 1 < Options.Count && SelectedIndex - 1 >= 0)
                        SelectedIndex -= 1;
                    else
                        SelectedIndex = Options.Count - 1;
                    break;
                case Direction.Forward:
                    if (SelectedIndex + 1 < Options.Count)
                        SelectedIndex += 1;
                    else
                        SelectedIndex = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            SelectedItemText.Text = Options[SelectedIndex];
            OnChange(Options[SelectedIndex], SelectedIndex);
        }

        /// <summary>
        ///  Changes the selected element to a given index.
        /// </summary>
        /// <param name="index"></param>
        public void SelectIndex(int index)
        {
            SelectedIndex = index;
            SelectedItemText.Text = Options[SelectedIndex];
        }
    }
}
