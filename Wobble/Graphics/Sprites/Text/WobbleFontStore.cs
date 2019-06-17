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
        public WobbleFontStore(int size, byte[] font)
        {
            DefaultSize = size;
            Store = DynamicSpriteFont.FromTtf(font, size);
        }

        /// <summary>
        ///     Adds a font to the store from a byte[]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        public void AddFont(string name, byte[] font) => Store.AddTtf(name, font);
    }
}