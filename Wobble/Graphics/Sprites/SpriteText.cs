using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites
{
    /// <inheritdoc />
    /// <summary>
    ///     Any drawable object that uses
    /// </summary>
    public class SpriteText : Drawable
    {
        /// <summary>
        ///     The Actual text of the text Box.
        /// </summary>
        private string _text = "";

        /// <summary>
        ///     The alignment of the text.
        /// </summary>
        public Alignment TextAlignment { get; set; } = Alignment.MidCenter;

        /// <summary>
        ///     The target scale of the text.
        /// </summary>
        public float TextScale { get; set; } = 1;

        /// <summary>
        ///     How the text will wrap/scale inside the text box
        /// </summary>
        public TextStyle TextBoxStyle { get; set; } = TextStyle.OverflowSingleLine;

        /// <summary>
        ///     The Rectangle of the rendered text inside the QuaverTextSprite.
        /// </summary>
        private DrawRectangle _globalTextVect = new DrawRectangle();

        /// <summary>
        ///     The position of the text box
        /// </summary>
        private Vector2 _textPos = Vector2.Zero;

        /// <summary>
        ///     The Local Rectangle of the rendered text inside the QuaverTextSprite. Used to reference Text Size.
        /// </summary>
        private DrawRectangle _textVect = new DrawRectangle();

        /// <summary>
        ///     The size of the rendered text box in a single row.
        /// </summary>
        private Vector2 _textSize;

        /// <summary>
        ///     The scale of the text.
        /// </summary>
        private float _textScale { get; set; } = 1;

        /// <summary>
        ///     The font of this object
        /// </summary>
        public SpriteFont Font { get; set; }

        /// <summary>
        ///     The text of this QuaverTextSprite
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The tint this Text Object will inherit.
        /// </summary>
        public Color TextColor
        {
            get => _tint;
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            }
        }
        private Color _tint = Color.White;

        /// <summary>
        ///     The transparency of this Text Object.
        /// </summary>
        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                _color = _tint * _alpha;

                if (!SetChildrenAlpha)
                    return;

                Children.ForEach(x =>
                {
                    var t = x.GetType();

                    if (t == typeof(Sprite))
                    {
                        var sprite = (Sprite)x;
                        sprite.Alpha = value;
                    }
                    else if (t == typeof(SpriteText))
                    {
                        var text = (SpriteText)x;
                        text.Alpha = value;
                    }
                });
            }
        }

        private float _alpha = 1f;

        /// <summary>
        ///     The color of this Text Object.
        /// </summary>
        private Color _color = Color.White;

        /// <summary>
        ///     Dictates if we want to set the alpha of the children to this one
        ///     if it is changed.
        /// </summary>
        public bool SetChildrenAlpha { get; set; }

        public SpriteText() => RectangleRecalculated += (o, e) => UpdateText();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (_textScale == 1)
                GameBase.Game.SpriteBatch.DrawString(Font, _text, _textPos, _color);
            else
                GameBase.Game.SpriteBatch.DrawString(Font, _text, _textPos, _color, 0, Vector2.One, Vector2.One * _textScale, SpriteEffects.None, 0);

            base.Draw(gameTime);
        }

        private void UpdateText()
        {
            //Update TextSize
            _textSize = Font.MeasureString(Text);
            _textScale = TextScale;

            // Update text with given textbox style
            switch (TextBoxStyle)
            {
                case TextStyle.OverflowMultiLine:
                    _text = WrapText(Text, true, true);
                    break;
                case TextStyle.WordwrapMultiLine:
                    _text = WrapText(Text, true);
                    break;
                case TextStyle.OverflowSingleLine:
                    _text = Text;
                    break;
                case TextStyle.WordwrapSingleLine:
                    _text = WrapText(Text, false);
                    break;
                case TextStyle.ScaledSingleLine:
                    _text = Text;
                    _textScale = ScaleText(AbsoluteSize, _textSize * TextScale) * TextScale;
                    break;
            }

            //Update TextRect
            _textVect.Width = _textSize.X * _textScale;
            _textVect.Height = _textSize.Y * _textScale;

            //Update GlobalTextRect
            _globalTextVect = GraphicsHelper.AlignRect(TextAlignment, _textVect, ScreenRectangle);
            _textPos.X = _globalTextVect.X;
            _textPos.Y = _globalTextVect.Y;
        }

        private float ScaleText(Vector2 boundary, Vector2 textboxsize)
        {
            var sizeYRatio = (boundary.Y / boundary.X) / (textboxsize.Y / textboxsize.X);
            if (sizeYRatio > 1)
                return (boundary.X / textboxsize.X);
            else
                return (boundary.Y / textboxsize.Y);
        }

        private string WrapText(string text, bool multiLine, bool overflow = false)
        {
            //Check if text is not short enough to fit on its on box
            if (Font.MeasureString(text).X < Width) return text;

            //Reference Variables
            var words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = Font.MeasureString(" ").X;
            var textline = 0;
            var MaxTextLines = 99; //todo: remove

            //Update Text
            foreach (var a in words)
            {
                var size = Font.MeasureString(a);
                if (linewidth + size.X < AbsoluteSize.X)
                {
                    linewidth += size.X + spaceWidth;
                }
                else if (multiLine)
                {
                    //Add new line
                    wrappedText.Append("\n");
                    linewidth = size.X + spaceWidth;

                    //Check if text wrap should continue
                    textline++;
                    if (textline >= MaxTextLines) break;
                }
                else break;
                wrappedText.Append(a + " ");
            }
            //Console.WriteLine("MAX: {0}, TOTAL {1}", MaxTextLines, textline);
            return wrappedText.ToString();
        }

        /// <summary>
        ///     Measures the size of the sprite.
        /// </summary>
        /// <returns></returns>
        public Vector2 MeasureString() => Font.MeasureString(Text) * TextScale;
    }

    /// <summary>
    ///     Used for SpriteText that determines how to display the text.
    /// </summary>
    public enum TextStyle
    {
        OverflowSingleLine,
        OverflowMultiLine,
        ScaledSingleLine,
        WordwrapSingleLine,
        WordwrapMultiLine
    }
}
