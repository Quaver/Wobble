using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Wobble.Assets;

namespace Wobble.Graphics.Sprites
{
    public class SpriteTextBitmap : Drawable
    {
        /// <summary>
        ///     The font used to draw the text
        /// </summary>
        public BitmapFont Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                DisplayedText = WrapText(Text);
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
                _text = value;
                DisplayedText = WrapText(value);
            }
        }

        /// <summary>
        ///     The text that'll be displayed on-screen (after wrapping)
        /// </summary>
        public string DisplayedText { get; private set; }

        /// <summary>
        ///     The maximum width of the text.
        /// </summary>
        private int _maxWidth = int.MaxValue;
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                DisplayedText = WrapText(Text);
            }
        }

        /// <summary>
        ///     The rotation of the text upon drawing it.
        /// </summary>
        private float _rotation;
        public float Rotation
        {
            get => _rotation;
            set => _rotation = MathHelper.ToRadians(value);
        }

        /// <summary>
        ///     Dictates if we want to set the alpha of the children as well.
        /// </summary>
        public bool SetChildrenAlpha { get; set; }

        /// <summary>
        ///     The transparency of this QuaverSprite.
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
                    if (x is Sprite sprite)
                    {
                        sprite.Alpha = value;
                    }
                });
            }
        }

        /// <summary>
        ///     The absolute color the text including alpha.
        /// </summary>
        private Color _tint = Color.White;
        public Color _color = Color.White;
        public Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            }
        }

        /// <summary>
        ///     Effects applied to the text when drawing it.
        /// </summary>
        public SpriteEffects Effects { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        public SpriteTextBitmap(BitmapFont font, string text)
        {
            Font = font;
            FontSize = font.LineHeight;
            Text = text;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            GameBase.Game.SpriteBatch.DrawString(Font, DisplayedText, AbsolutePosition, _color, Rotation,
                Vector2.Zero, new Vector2((float)FontSize / Font.LineHeight, (float)FontSize / Font.LineHeight),
                Effects, 0, null);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (SpriteBatchOptions != null)
            {
                // If we actually have new SpriteBatchOptions to use,then
                // we want to end the previous SpriteBatch.
                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception)
                {
                    // ignored
                }

                GameBase.DefaultSpriteBatchInUse = false;

                // Begin the new SpriteBatch
                SpriteBatchOptions.Begin();

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
                catch (Exception)
                {
                    // ignored
                }

                // Begin the default spriteBatch
                GameBase.DefaultSpriteBatchOptions.Begin();
                GameBase.DefaultSpriteBatchInUse = true;

                DrawToSpriteBatch();
            }
            // This must mean that the default SpriteBatch is in use, so we can just go ahead and draw the object.
            else
            {
                try
                {
                    DrawToSpriteBatch();
                }
                catch (Exception e)
                {
                    GameBase.DefaultSpriteBatchOptions.Begin();
                    GameBase.DefaultSpriteBatchInUse = true;

                    DrawToSpriteBatch();
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Performs text wrapping based on 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string WrapText(string text)
        {
            if (Font.MeasureString(text).Width * ((float) FontSize / Font.LineHeight) < MaxWidth)
            {
                Width = Font.MeasureString(text).Width * ((float) FontSize / Font.LineHeight);
                Height = Font.MeasureString(text).Height * ((float)FontSize / Font.LineHeight);

                return text;
            }

            var words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = Font.MeasureString(" ").Width * ((float)FontSize / Font.LineHeight);

            for (var i = 0; i < words.Length; ++i)
            {
                Vector2 size = Font.MeasureString(words[i]) * ((float)FontSize / Font.LineHeight);

                if (linewidth + size.X < MaxWidth)
                    linewidth += size.X + spaceWidth;
                else
                {
                    if (i != 0)
                        wrappedText.Append("\n");

                    linewidth = size.X + spaceWidth;
                }
                wrappedText.Append(words[i]);
                wrappedText.Append(" ");
            }

            Width = Font.MeasureString(wrappedText.ToString()).Width * ((float)FontSize / Font.LineHeight);
            Height = Font.MeasureString(wrappedText.ToString()).Height * ((float)FontSize / Font.LineHeight);

            return wrappedText.ToString();
        }
    }
}
