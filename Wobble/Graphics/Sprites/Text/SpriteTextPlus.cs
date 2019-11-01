using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private TextAlignment _textAlignment = TextAlignment.Left;
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
        ///     The maximal width of the text; the text will be wrapped to fit.
        /// </summary>
        private float? _maxWidth = null;
        public float? MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (value == _maxWidth)
                    return;

                _maxWidth = value;
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

            var lines = Text.Split('\n').ToList();
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                var lineSprite = new SpriteTextPlusLine(Font, line, FontSize);

                if (MaxWidth != null && lineSprite.Width > MaxWidth)
                {
                    Debug.Assert(line.Length > 0);

                    // Try to split the line on spaces to fit it into MaxWidth.
                    var spaces = new List<int>();
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (char.IsWhiteSpace(line[i]))
                            spaces.Add(i);
                    }

                    // Binary search would be great for the next two (as long as we're assuming that
                    // more characters == longer lines, which will not hold for complex scripts,
                    // which aren't supported yet anyway), but C# doesn't have a built-in method
                    // for binary search by an arbitrary predicate. So I guess we'll just go with a regular find-last,
                    // which can be slower, but has a bonus of not making any of the aforementioned assumptions.
                    var splitOnIndex = spaces.FindLastIndex(spacePosition =>
                    {
                        var lineBeforeSpace = line.Substring(0, spacePosition);
                        var sprite = new SpriteTextPlusLine(Font, lineBeforeSpace, FontSize);
                        var lineWidth = sprite.Width;
                        sprite.Dispose();
                        return lineWidth <= MaxWidth;
                    });

                    // It's always initialized, but the C# compiler isn't smart enough to figure that out.
                    int? nextLineStart = null;

                    if (splitOnIndex == -1)
                    {
                        // Splitting even on the first whitespace gives a line that's too long (or there are no spaces at all),
                        // so split on any character.
                        //
                        // Splitting on arbitrary characters like this is questionable, because while even in English
                        // splitting mid-word can produce nonsensical results, in some complex scripts it doesn't
                        // make any sense whatsoever to split on something that's not a space. But, once again,
                        // we don't support complex scripts yet, and this is a decent enough fallback to make sure
                        // the lines don't get too off max width.
                        var lastIndex = line.Length;
                        if (spaces.Count > 0)
                            lastIndex = spaces[0];

                        for (var i = lastIndex; i != 0; i--)
                        {
                            var lineCut = line.Substring(0, i);
                            var sprite = new SpriteTextPlusLine(Font, lineCut, FontSize);

                            // If we're left with 1 character, just go with it even if we're over MaxWidth.
                            if (sprite.Width > MaxWidth && i > 1)
                            {
                                sprite.Dispose();
                                continue;
                            }

                            lineSprite.Dispose();
                            lineSprite = sprite;
                            nextLineStart = i;
                            break;
                        }
                    }
                    else
                    {
                        var lineBeforeSpace = line.Substring(0, spaces[splitOnIndex]);
                        lineSprite.Dispose();
                        lineSprite = new SpriteTextPlusLine(Font, lineBeforeSpace, FontSize);
                        nextLineStart = spaces[splitOnIndex] + 1; // Skip over the space that we replaced.
                    }

                    // Insert the remaining part of the line into the list to be iterated over next.
                    Debug.Assert(nextLineStart != null);
                    var lineAfterSpace = line.Substring(nextLineStart.Value);
                    lines.Insert(lineIndex + 1, lineAfterSpace);
                }

                lineSprite.Parent = this;
                lineSprite.Alignment = ConvertTextAlignment();
                lineSprite.Y = height;
                lineSprite.UsePreviousSpriteBatchOptions = true;
                lineSprite.Tint = Tint;
                lineSprite.Alpha = Alpha;

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
                case TextAlignment.Left:
                    return Alignment.TopLeft;
                case TextAlignment.Center:
                    return Alignment.TopCenter;
                case TextAlignment.Right:
                    return Alignment.TopRight;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}