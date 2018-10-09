using System;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;

namespace Wobble.Graphics.UI.Buttons
{
    [Obsolete("TextButton is obselete, use BitmapTextButton")]
    public class TextButton : ImageButton
    {
        /// <summary>
        ///     The sprite text displayed on the button.
        /// </summary>
        public SpriteText Text { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new TextButton
        /// </summary>
        /// <param name="image"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="textScale"></param>
        /// <param name="clickAction"></param>
        public TextButton(Texture2D image, SpriteFont font, string text, float textScale = 1.0f, EventHandler clickAction = null)
            : base(image, clickAction) => Text = new SpriteText(font, text)
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            TextScale = textScale,
            TextAlignment = Alignment.MidCenter,
            Y = 1
        };
    }
}