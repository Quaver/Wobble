using System;
using System.Collections.Generic;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites.Text
{
    public class WobbleFontStore
    {
        private float _fontSize;
        private readonly FontSystem _fontSystem;

        /// <summary>
        ///     All of the contained fonts at different sizes
        /// </summary>
        public DynamicSpriteFont Store { get; set; }

        /// <summary>
        ///     The size the font was initially created ad
        /// </summary>
        public int DefaultSize { get; }

        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                Store = _fontSystem.GetFont(value);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="font"></param>
        /// <param name="addedFonts"></param>
        public WobbleFontStore(int size, byte[] font, Dictionary<string, byte[]> addedFonts = null)
        {
            DefaultSize = size;

            _fontSystem = new FontSystem();
            _fontSystem.AddFont(font);
            Store = _fontSystem.GetFont(size);

            if (addedFonts == null)
            {
                FontSize = size;
                return;
            }

            foreach (var f in addedFonts)
                AddFont(f.Key, f.Value);
            FontSize = size;
        }

        /// <summary>
        ///     Adds a font to the store from a byte[]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        public void AddFont(string name, byte[] font) => _fontSystem.AddFont(font);
    }
}