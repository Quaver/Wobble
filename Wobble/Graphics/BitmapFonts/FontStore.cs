using System.Drawing;

namespace Wobble.Graphics.BitmapFonts
{
    public struct FontStore
    {
        /// <summary>
        ///     The name of the font.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///    The font family
        /// </summary>
        public FontFamily Family { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="family"></param>
        public FontStore(string name, FontFamily family)
        {
            Name = name;
            Family = family;
        }
    }
}