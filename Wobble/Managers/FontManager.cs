using System;
using System.Collections.Generic;
using MonoGame.Extended.BitmapFonts;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;

namespace Wobble.Managers
{
    public static class FontManager
    {
        /// <summary>
        /// </summary>
        public static Dictionary<string, BitmapFont> BitmapFonts { get; } = new Dictionary<string, BitmapFont>();

        /// <summary>
        /// </summary>
        public static Dictionary<string, WobbleFontStore> WobbleFonts { get; } = new Dictionary<string, WobbleFontStore>();

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

        /// <summary>
        ///     Loads and caches a WobbleFont
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static void CacheWobbleFont(string name, WobbleFontStore font)
        {
            if (WobbleFonts.ContainsKey(name))
                throw new ArgumentException("A font with this name already exists!");

            WobbleFonts.Add(name, font);
            Logger.Debug($"Loaded font: {name}", LogType.Runtime);
        }

        /// <summary>
        ///     Retrieves a WobbleFont if cached
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static WobbleFontStore GetWobbleFont(string name) => WobbleFonts[name];
    }
}