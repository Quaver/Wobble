using System;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Wobble.Graphics.Sprites
{
    public class SpriteText : SpriteTextPlus
    {
        private string _font;
        private Alignment _textAlignment = Alignment.MidLeft;

        /// <summary>
        ///     The name of the cached font to use.
        /// </summary>
        public new string Font
        {
            get => _font;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Font must be not null or empty.");

                _font = value;
                base.Font = FontManager.GetWobbleFont(value);
            }
        }

        /// <summary>
        ///     The maximum width before the text wraps to the next line.
        /// </summary>
        public new int MaxWidth
        {
            get => base.MaxWidth == null ? int.MaxValue : (int) base.MaxWidth.Value;
            set => base.MaxWidth = value == int.MaxValue ? (float?) null : value;
        }

        /// <summary>
        ///     The legacy alignment shape used by SpriteText.
        /// </summary>
        public new Alignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                base.TextAlignment = ConvertTextAlignment(value);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="maxWidth">The maximum width before it wraps to the next line</param>
        /// <exception cref="ArgumentException"></exception>
        public SpriteText(string font, string text, int fontSize, int maxWidth = int.MaxValue)
            : base(ResolveFont(font), text, fontSize)
        {
            if (string.IsNullOrEmpty(font))
                throw new ArgumentException("Font must be not null or empty.");

            _font = font;
            MaxWidth = maxWidth;
        }

        private static WobbleFontStore ResolveFont(string font)
        {
            if (string.IsNullOrEmpty(font))
                throw new ArgumentException("Font must be not null or empty.");

            return FontManager.GetWobbleFont(font);
        }

        private static global::Wobble.Graphics.Sprites.Text.TextAlignment ConvertTextAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.TopLeft:
                case Alignment.MidLeft:
                case Alignment.BotLeft:
                    return global::Wobble.Graphics.Sprites.Text.TextAlignment.Left;
                case Alignment.TopCenter:
                case Alignment.MidCenter:
                case Alignment.BotCenter:
                    return global::Wobble.Graphics.Sprites.Text.TextAlignment.Center;
                case Alignment.TopRight:
                case Alignment.MidRight:
                case Alignment.BotRight:
                    return global::Wobble.Graphics.Sprites.Text.TextAlignment.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }
    }
}
