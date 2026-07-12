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

        public bool EnableTabularNumbers { get; }

        public WobbleFontFace(byte[] data, int index = 0,
            int weight = FontWeight.Regular, bool enableTabularNumbers = false)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Index = index;
            Weight = weight;
            EnableTabularNumbers = enableTabularNumbers;
        }
    }

    public class WobbleFontStore
    {
        private float _fontSize;
        private FontSystem _fontSystem;
        private FreeTypeFontLoader _fontLoader;

        public event EventHandler Changed;

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
            : this(size, font, 0, addedFonts)
        {
        }

        public WobbleFontStore(int size, byte[] font,
            int implicitFontSizeReduction, Dictionary<string, byte[]> addedFonts = null)
            : this(size,
                new WobbleFontFace(font),
                implicitFontSizeReduction,
                ToFontFaces(addedFonts))
        {
        }

        public WobbleFontStore(int size, byte[] font, Dictionary<string, WobbleFontFace> addedFonts)
            : this(size, font, addedFonts, 0)
        {
        }

        public WobbleFontStore(int size, byte[] font, Dictionary<string, WobbleFontFace> addedFonts,
            int implicitFontSizeReduction)
            : this(size,
                new WobbleFontFace(font),
                implicitFontSizeReduction,
                addedFonts)
        {
        }

        public WobbleFontStore(int size, WobbleFontFace font,
            Dictionary<string, WobbleFontFace> addedFonts = null)
            : this(size, font, 0, addedFonts)
        {
        }

        public WobbleFontStore(int size, WobbleFontFace font,
            int implicitFontSizeReduction,
            Dictionary<string, WobbleFontFace> addedFonts = null)
        {
            DefaultSize = size;

            Load(font, implicitFontSizeReduction, addedFonts);
        }

        /// <summary>
        ///     Adds a font to the store from a byte[]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        public void AddFont(string name, byte[] font, int index = 0,
            int weight = FontWeight.Regular, int implicitFontSizeReduction = 0,
            bool enableTabularNumbers = false)
        {
            _fontLoader.Register(font, index, weight, implicitFontSizeReduction, enableTabularNumbers);
            _fontSystem.AddFont(font);
        }

        public void Reload(WobbleFontFace font, int implicitFontSizeReduction,
            Dictionary<string, WobbleFontFace> addedFonts = null)
        {
            Load(font, implicitFontSizeReduction, addedFonts);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void Load(WobbleFontFace font, int implicitFontSizeReduction,
            Dictionary<string, WobbleFontFace> addedFonts)
        {
            _fontLoader = new FreeTypeFontLoader();
            _fontSystem = new FontSystem(new FontSystemSettings { FontLoader = _fontLoader });

            AddFont(string.Empty, font.Data, font.Index, font.Weight, implicitFontSizeReduction,
                font.EnableTabularNumbers);

            if (addedFonts != null)
            {
                foreach (var f in addedFonts)
                    AddFont(f.Key, f.Value.Data, f.Value.Index, f.Value.Weight, implicitFontSizeReduction,
                        f.Value.EnableTabularNumbers);
            }

            FontSize = _fontSize == 0 ? DefaultSize : _fontSize;
        }

        private static Dictionary<string, WobbleFontFace> ToFontFaces(Dictionary<string, byte[]> fonts)
        {
            if (fonts == null)
                return null;

            var result = new Dictionary<string, WobbleFontFace>();

            foreach (var font in fonts)
                result.Add(font.Key,
                    new WobbleFontFace(font.Value));

            return result;
        }
    }
}
