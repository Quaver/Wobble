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
        ///     The string of text to be displayed.
        /// </summary>
        private string _text = "";
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
        ///     The font used for this text.
        /// </summary>
        private SpriteFont _font;
        public SpriteFont Font
        {
            get => _font;
            set
            {
                _font = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The alignment of the text.
        /// </summary>
        private Alignment _textAlignment = Alignment.MidCenter;
        public Alignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private float _textScale = 1;
        public float TextScale
        {
            get => _textScale;
            set
            {
                _textScale = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     How the text will wrap/scale inside the text box
        /// </summary>
        private TextStyle _style = TextStyle.OverflowSingleLine;
        public TextStyle Style
        {
            get => _style;
            set
            {
                _style = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The tint this Text Object will inherit.
        /// </summary>
        private Color _tint = Color.White;
        private Color _color = Color.White;
        public Color TextColor
        {
            get => _tint;
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            }
        }

        /// <summary>
        ///     The transparency of this Text Object.
        /// </summary>
       private float _alpha = 1f;
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

        /// <summary>
        ///     Dictates if we want to set the alpha of the children to this one
        ///     if it is changed.
        /// </summary>
        public bool SetChildrenAlpha { get; set; }

        /// <summary>
        ///     The Rectangle of the rendered text inside the TextSprite
        /// </summary>
        private DrawRectangle TextScreenRectangle { get; set; } = new DrawRectangle();

        /// <summary>
        ///     The position of the text box
        /// </summary>
        private Vector2 TextPosition { get; set; } = Vector2.Zero;

        /// <summary>
        ///     The Local Rectangle of the rendered text inside the SpriteText. Used to reference Text Size.
        /// </summary>
        private DrawRectangle TextRelativeRectangle { get; set; } = new DrawRectangle();

        /// <summary>
        ///     The size of the rendered text box in a single row.
        /// </summary>
        private Vector2 TextSize { get; set; }

        /// <summary>
        ///     Used when wrapping lines. Gives slightly less/more line space before wrapping to the next line.
        /// </summary>
        public int Padding { get; set; }

        /// <summary>
        ///    When a SpriteText is created, when want to hook onto whenever the rect is being recalced event
        ///    and update the text accordingly with the new values.
        /// </summary>
        public SpriteText(SpriteFont font, string text, float scale = 1.0f)
        {
            Font = font;
            Text = text;
            TextScale = scale;
            RectangleRecalculated += (o, e) => UpdateText();
        }

        /// <summary>
        ///     Create SpriteFont with set size.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="scale"></param>
        public SpriteText(SpriteFont font, string text, ScalableVector2 size, float scale = 1.0f)
        {
            Size = size;
            Font = font;
            Text = text;
            TextScale = scale;
            RectangleRecalculated += (o, e) => UpdateText();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
            {
                base.Draw(gameTime);
                return;
            }

            if (SpriteBatchOptions != null)
            {
                // If we actually have new SpriteBatchOptions to use,then
                // we want to end the previous SpriteBatch.
                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception e)
                {
                    // ignored
                }

                GameBase.DefaultSpriteBatchInUse = false;

                // Begin the new SpriteBatch
                SpriteBatchOptions.Begin();

                // Draw the object.
                DrawToSpriteBatch();
            }
            // If the default spritebatch isn't used, we'll want to use it here and draw the sprite.
            else if (!GameBase.DefaultSpriteBatchInUse && !UsePreviousSpriteBatchOptions)
            {
                try
                {
                    // End the previous SpriteBatch.
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception e)
                {
                    // ignored
                }

                // Begin the default spriteBatch
                GameBase.DefaultSpriteBatchOptions.Begin();
                GameBase.DefaultSpriteBatchInUse = true;

                // Draw the object.
                DrawToSpriteBatch();
            }
            // This must mean that the default SpriteBatch is in use, so we can just go ahead and draw the object.
            else
            {
                DrawToSpriteBatch();
            }

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Draws the text to the SpriteBatch.
        /// </summary>
        protected override void DrawToSpriteBatch()
        {
            if (Math.Abs(_textScale - 1) < 0.01)
                GameBase.Game.SpriteBatch.DrawString(Font, _text, TextPosition, _color);
            else
                GameBase.Game.SpriteBatch.DrawString(Font, _text, TextPosition, _color, 0, Vector2.One, Vector2.One * _textScale, SpriteEffects.None, 0);
        }

        /// <summary>
        ///     Updates the text to make sure it's size, text, scale, and position are correct
        ///     This is usually called whenever a property of the text is changed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void UpdateText()
        {
            // Update TextSize
            TextSize = Font.MeasureString(Text);
            _textScale = TextScale;

            // Update text with given textbox style
            switch (Style)
            {
                case TextStyle.OverflowMultiLine:
                    _text = WrapText(Text, true);
                    break;
                case TextStyle.WordwrapMultiLine:
                    _text = WrapText(Text);
                    break;
                case TextStyle.OverflowSingleLine:
                    _text = Text;
                    break;
                case TextStyle.WordwrapSingleLine:
                    _text = WrapText(Text, false);
                    break;
                case TextStyle.ScaledSingleLine:
                    _text = Text;
                    _textScale = ScaleText(AbsoluteSize, TextSize * TextScale) * TextScale;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Update Relative Text Rectangle
            TextRelativeRectangle.Width = TextSize.X * _textScale;
            TextRelativeRectangle.Height = TextSize.Y * _textScale;

            // Update Screen Text Rectangle
            TextScreenRectangle = GraphicsHelper.AlignRect(TextAlignment, TextRelativeRectangle, ScreenRectangle);
            TextPosition = new Vector2(TextScreenRectangle.X, TextScreenRectangle.Y);
        }

        /// <summary>
        ///     Returns the correct scale for the text.
        ///     TODO: Add proper documentation.
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="textboxsize"></param>
        /// <returns></returns>
        private static float ScaleText(Vector2 boundary, Vector2 textboxsize)
        {
            var sizeYRatio = boundary.Y / boundary.X / (textboxsize.Y / textboxsize.X);
            return sizeYRatio > 1 ? boundary.X / textboxsize.X : boundary.Y / textboxsize.Y;
        }

        /// <summary>
        ///     When the text is updated, depending on the style, this will format the text correctly.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="multiLine"></param>
        /// <returns></returns>
        private string WrapText(string text, bool multiLine)
        {
            // Check if text is not short enough to fit on its on box
            if (Font.MeasureString(text).X < Width)
                return text;

            // Reference Variables
            var words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = Font.MeasureString(" ").X;
            var textline = 0;
            const int maxTextLines = 99;

            foreach (var a in words)
            {
                var size = Font.MeasureString(a);

                if (linewidth + size.X < Width)
                {
                    linewidth += size.X + spaceWidth;
                }
                else if (multiLine)
                {
                    // Add new line
                    wrappedText.Append("\n");

                    linewidth = size.X + spaceWidth;

                    // Check if text wrap should continue
                    textline++;

                    if (textline >= maxTextLines)
                        break;
                }
                else
                    break;

                wrappedText.Append(a + " ");
            }

            return wrappedText.ToString();
        }

        private string WrapText(string text)
        {
            text = text.Replace("\n", "");
            if(MeasureString(text).X < Width - Padding) {
                return text;
            }

            var words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = MeasureString(" ").X;
            for(var i = 0; i < words.Length; ++i) {
                var size = MeasureString(words[i]);
                if(linewidth + size.X < Width - Padding) {
                    linewidth += size.X + spaceWidth;
                } else {
                    wrappedText.Append("\n");
                    linewidth = size.X + spaceWidth;
                }
                wrappedText.Append(words[i]);
                wrappedText.Append(" ");
            }

            return wrappedText.ToString();
        }

        /// <summary>
        ///     Measures the size of the sprite.
        /// </summary>
        /// <returns></returns>
        public Vector2 MeasureString() => Font.MeasureString(Text) * TextScale;

        /// <summary>
        ///     Measures size of inputted text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Vector2 MeasureString(string text) => Font.MeasureString(text) * TextScale;

        /// <summary>
        ///     Fades the sprite to a given color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        public void FadeToColor(Color color, double dt, float scale)
        {
            var r = MathHelper.Lerp(TextColor.R, color.R, (float) Math.Min(dt / scale, 1));
            var g = MathHelper.Lerp(TextColor.G, color.G, (float) Math.Min(dt / scale, 1));
            var b = MathHelper.Lerp(TextColor.B, color.B, (float) Math.Min(dt / scale, 1));

            TextColor = new Color((int)r, (int)g, (int)b);
        }
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
