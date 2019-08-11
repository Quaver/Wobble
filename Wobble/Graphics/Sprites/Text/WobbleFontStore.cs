using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace Wobble.Graphics.Sprites.Text
{
    public class WobbleFontStore
    {
        /// <summary>
        ///     All of the contained fonts at different sizes
        /// </summary>
        public DynamicSpriteFont Store { get; }

        /// <summary>
        ///     The size the font was initially created ad
        /// </summary>
        public int DefaultSize { get; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="font"></param>
        /// <param name="addedFonts"></param>
        public WobbleFontStore(int size, byte[] font, Dictionary<string, byte[]> addedFonts = null)
        {
            DefaultSize = size;
            Store = DynamicSpriteFont.FromTtf(font, size);

            if (addedFonts == null)
                return;

            foreach (var f in addedFonts)
                AddFont(f.Key, f.Value);
        }

        /// <summary>
        ///     Adds a font to the store from a byte[]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        public void AddFont(string name, byte[] font) => Store.AddTtf(name, font);
    }
}