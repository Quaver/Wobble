using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Wobble.Graphics.UI.Buttons
{
    public class BitmapTextButton : ImageButton
    {
        /// <summary>
        ///     The sprite text inside of the button
        /// </summary>
        public SpriteText Text { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="clickAction"></param>
        public BitmapTextButton(Texture2D image, string font, string text, int fontSize, EventHandler clickAction = null)
            : base(image, clickAction) => Text = new SpriteText(font, text, fontSize)
        {
            Parent = this,
            Alignment = Alignment.MidCenter
        };
    }
}