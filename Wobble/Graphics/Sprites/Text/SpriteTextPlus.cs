using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Animations;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlus : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (value == _fontSize)
                    return;

                _fontSize = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                _text = value ?? "";

                RefreshText();
            }
        }

        /// <summary>
        ///     The tint this QuaverSprite will inherit.
        /// </summary>
        private Color _tint = Color.White;
        public Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;

                Children.ForEach(x =>
                {
                    if (x is Sprite sprite)
                    {
                        sprite.Tint = value;
                    }
                });
            }
        }

        /// <summary>
        ///     The alignment of the text
        /// </summary>
        private TextAlignment _textAlignment = TextAlignment.LeftToRight;
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (value == _textAlignment)
                    return;

                _textAlignment = value;
                RefreshText();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlus(WobbleFontStore font, string text, int size = 0)
        {
            Font = font;
            Text = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
            SetChildrenAlpha = true;
        }

        /// <summary>
        /// </summary>
        private void RefreshText()
        {
            for (var i = Children.Count - 1; i >= 0; i--)
                Children[i].Destroy();

            float width = 0, height = 0;

            foreach (var line in Text.Split('\n'))
            {
                var lineSprite = new SpriteTextPlusLine(Font, line, FontSize)
                {
                    Parent = this,
                    Alignment = ConvertTextAlignment(),
                    Y = height,
                    UsePreviousSpriteBatchOptions = true,
                    Tint = Tint,
                    Alpha = Alpha,
                };

                width = Math.Max(width, lineSprite.Width);

                Font.Store.Size = FontSize;
                height += Font.Store.GetLineHeight();
            }

            Size = new ScalableVector2(width, height);
        }

        /// <summary>
        ///     Truncates the text with an elipsis according to <see cref="maxWidth"/>
        /// </summary>
        /// <param name="maxWidth"></param>
        public void TruncateWithEllipsis(int maxWidth)
        {
            var originalText = Text;

            while (Width > maxWidth)
                Text = Text.Substring(0, Text.Length - 1);

            if (Text != originalText)
                Text += "...";
        }

        public override void DrawToSpriteBatch()
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Alignment ConvertTextAlignment()
        {
            switch (TextAlignment)
            {
                case TextAlignment.LeftToRight:
                    return Alignment.TopLeft;
                case TextAlignment.Center:
                    return Alignment.TopCenter;
                case TextAlignment.RightToLeft:
                    return Alignment.TopRight;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}