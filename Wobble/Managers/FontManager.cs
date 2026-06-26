using System;
using System.Collections.Generic;
using System.Linq;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;

namespace Wobble.Managers
{
    public static class FontManager
    {
        /// <summary>
        /// </summary>
        public static Dictionary<string, WobbleFontStore> WobbleFonts { get; } = new Dictionary<string, WobbleFontStore>();

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
        ///     Loads and caches a WobbleFont from raw font bytes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fontBytes"></param>
        /// <param name="defaultSize"></param>
        public static void AddFont(string name, byte[] fontBytes, int defaultSize = 20)
        {
            CacheWobbleFont(name, new WobbleFontStore(defaultSize, fontBytes));
        }

        /// <summary>
        ///     Loads and caches a WobbleFont from a font file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        /// <param name="defaultSize"></param>
        public static void AddFont(string name, string filePath, int defaultSize = 20)
        {
            AddFont(name, System.IO.File.ReadAllBytes(filePath), defaultSize);
        }

        /// <summary>
        ///     Retrieves a WobbleFont if cached.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static WobbleFontStore GetWobbleFont(string name)
        {
            if (WobbleFonts.ContainsKey(name))
                return WobbleFonts[name];

            var fallback = WobbleFonts.Values.FirstOrDefault();

            if (fallback != null)
                return fallback;

            throw new ArgumentException($"Font '{name}' has not been cached in FontManager.");
        }
    }
}
