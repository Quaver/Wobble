using System;
using System.Collections.Generic;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites.Text
{
    public class WobbleFontFace
    {
        public byte[] Data { get; }

        public int Index { get; }

        public int Weight { get; }

        public WobbleFontFace(byte[] data, int index = 0, int weight = FontWeight.Regular)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Index = index;
            Weight = weight;
        }
    }

    public class WobbleFontStore
    {
        private float _fontSize;
        private readonly FontSystem _fontSystem;
        private readonly FreeTypeFontLoader _fontLoader;

        static WobbleFontStore()
        {
            FontSystemDefaults.FontResolutionFactor = 1f;
            FontSystemDefaults.KernelWidth = 0;
            FontSystemDefaults.KernelHeight = 0;
            FontSystemDefaults.GlyphRenderResult = GlyphRenderResult.NonPremultiplied;
        }

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
            : this(size, new WobbleFontFace(font), ToFontFaces(addedFonts))
        {
        }

        public WobbleFontStore(int size, byte[] font, Dictionary<string, WobbleFontFace> addedFonts)
            : this(size, new WobbleFontFace(font), addedFonts)
        {
        }

        public WobbleFontStore(int size, WobbleFontFace font, Dictionary<string, WobbleFontFace> addedFonts = null)
        {
            DefaultSize = size;

            _fontLoader = new FreeTypeFontLoader();
            _fontSystem = new FontSystem(new FontSystemSettings { FontLoader = _fontLoader });
            AddFont(string.Empty, font.Data, font.Index, font.Weight);
            Store = _fontSystem.GetFont(size);

            if (addedFonts == null)
            {
                FontSize = size;
                return;
            }

            foreach (var f in addedFonts)
                AddFont(f.Key, f.Value.Data, f.Value.Index, f.Value.Weight);
            FontSize = size;
        }

        /// <summary>
        ///     Adds a font to the store from a byte[]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        public void AddFont(string name, byte[] font, int index = 0, int weight = FontWeight.Regular)
        {
            _fontLoader.Register(font, index, weight);
            _fontSystem.AddFont(font);
        }

        private static Dictionary<string, WobbleFontFace> ToFontFaces(Dictionary<string, byte[]> fonts)
        {
            if (fonts == null)
                return null;

            var result = new Dictionary<string, WobbleFontFace>();

            foreach (var font in fonts)
                result.Add(font.Key, new WobbleFontFace(font.Value));

            return result;
        }
    }
}
