using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Window;

namespace Wobble.Graphics.UI.Buttons
{
    public class TextButton : ImageButton
    {
        /// <summary>
        ///     The sprite text inside of the button
        /// </summary>
        public SpriteTextPlus Text { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="clickAction"></param>
        public TextButton(Texture2D image, string font, string text, int fontSize, EventHandler clickAction = null)
            : base(image, clickAction)
        {
            Text = new SpriteTextPlus(font, text, fontSize)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
            };

            Size = Text.Size;
        }
    }
}
