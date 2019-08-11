using SpriteFontPlus;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLine : Sprite
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
                _fontSize = value;
                RefreshSize();
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
                RefreshSize();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLine(WobbleFontStore font, string text, int size = 0)
        {
            Font = font;
            Text = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            Font.Store.Size = FontSize;
            GameBase.Game.SpriteBatch.DrawString(Font.Store, Text, AbsolutePosition, _color);
        }

        private void RefreshSize()
        {
            Font.Store.Size = FontSize;

            var (x, y) = Font.Store.MeasureString(Text);
            Size = new ScalableVector2(x, y);
        }
    }
}