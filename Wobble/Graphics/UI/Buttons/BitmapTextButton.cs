using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Window;

namespace Wobble.Graphics.UI.Buttons
{
    public class BitmapTextButton : ImageButton
    {
        /// <summary>
        ///     The sprite text inside of the button
        /// </summary>
        public SpriteTextBitmap Text { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="textScale"></param>
        /// <param name="fontSize"></param>
        /// <param name="textColor"></param>
        /// <param name="clickAction"></param>
        public BitmapTextButton(Texture2D image, string font, string text, float textScale, int fontSize,
            Color textColor, EventHandler clickAction = null) : base(image, clickAction)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            Text = new SpriteTextBitmap(font, text, fontSize, textColor, Alignment.MidCenter, (int) WindowManager.Width)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true
            };

            Text.Size = new ScalableVector2(Text.Width * textScale, Text.Height * textScale);
        }
    }
}