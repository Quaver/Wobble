using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLineRaw : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private float _fontSize;
        public float FontSize
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
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0)
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

            Font.FontSize = FontSize;
            Font.Store.DrawText(GameBase.Game.SpriteBatch, Text, AbsolutePosition, _color, scale: AbsoluteScale);
        }

        private void RefreshSize()
        {
            Font.FontSize = FontSize;

            var (x, y) = Font.Store.MeasureString(Text);
            Size = new ScalableVector2(x, y);
        }
    }
}