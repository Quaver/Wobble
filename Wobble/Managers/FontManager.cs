using System.Collections.Generic;
using MonoGame.Extended.BitmapFonts;

namespace Wobble.Managers
{
    public static class FontManager
    {
        /// <summary>
        /// </summary>
        public static Dictionary<string, BitmapFont> BitmapFonts { get; }

        /// <summary>
        ///     Loads and caches a bitmap font
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BitmapFont LoadBitmapFont(string name)
        {
            if (BitmapFonts.ContainsKey(name))
                return BitmapFonts[name];

            var font = GameBase.Game.Content.Load<BitmapFont>(name);
            BitmapFonts.Add(name, font);

            return font;
        }
    }
}