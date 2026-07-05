using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using StbTrueTypeSharp;
using static StbTrueTypeSharp.StbTrueType;

namespace Wobble.Graphics.Sprites.Text
{
    internal class IndexedStbTrueTypeFontLoader : IFontLoader
    {
        private readonly Dictionary<byte[], int> _indexes = new Dictionary<byte[], int>();

        public void Register(byte[] data, int index)
        {
            _indexes[data] = index;
        }

        public IFontSource Load(byte[] data)
        {
            int index;
            if (!_indexes.TryGetValue(data, out index))
                index = 0;

            return new IndexedStbTrueTypeFontSource(data, index);
        }
    }

    internal unsafe class IndexedStbTrueTypeFontSource : IFontSource
    {
        private readonly IntPtr _dataPointer;
        private readonly stbtt_fontinfo _font;
        private readonly int _ascent;
        private readonly int _descent;
        private readonly int _lineGap;
        private bool _disposed;

        public IndexedStbTrueTypeFontSource(byte[] data, int index)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _dataPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, _dataPointer, data.Length);

            var dataPointer = (byte*)_dataPointer;
            var offset = stbtt_GetFontOffsetForIndex(dataPointer, index);

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "The font does not contain this face index.");

            _font = new stbtt_fontinfo();

            if (stbtt_InitFont(_font, dataPointer, offset) == 0)
                throw new ArgumentException("Unable to initialize font data.", nameof(data));

            int ascent;
            int descent;
            int lineGap;
            stbtt_GetFontVMetrics(_font, &ascent, &descent, &lineGap);
            _ascent = ascent;
            _descent = descent;
            _lineGap = lineGap;
        }

        ~IndexedStbTrueTypeFontSource()
        {
            Dispose(false);
        }

        public void GetMetricsForSize(float fontSize, out int ascent, out int descent, out int lineHeight)
        {
            var scale = CalculateScale(fontSize);
            ascent = (int)(_ascent * scale);
            descent = (int)(_descent * scale);
            lineHeight = (int)((_ascent - _descent + _lineGap) * scale);
        }

        public int? GetGlyphId(int codepoint)
        {
            var glyph = stbtt_FindGlyphIndex(_font, codepoint);
            return glyph == 0 ? (int?)null : glyph;
        }

        public void GetGlyphMetrics(int glyphId, float fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
        {
            int advanceWidth;
            int leftSideBearing;
            int localX0;
            int localY0;
            int localX1;
            int localY1;
            var scale = CalculateScale(fontSize);

            stbtt_GetGlyphHMetrics(_font, glyphId, &advanceWidth, &leftSideBearing);
            stbtt_GetGlyphBitmapBox(_font, glyphId, scale, scale, &localX0, &localY0, &localX1, &localY1);

            advance = (int)(advanceWidth * scale);
            x0 = localX0;
            y0 = localY0;
            x1 = localX1;
            y1 = localY1;
        }

        public void RasterizeGlyphBitmap(int glyphId, float fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
        {
            var scale = CalculateScale(fontSize);

            fixed (byte* bufferPointer = buffer)
                stbtt_MakeGlyphBitmap(_font, bufferPointer + startIndex, outWidth, outHeight, outStride, scale, scale, glyphId);
        }

        public int GetGlyphKernAdvance(int previousGlyphId, int glyphId, float fontSize)
        {
            return (int)(stbtt_GetGlyphKernAdvance(_font, previousGlyphId, glyphId) * CalculateScale(fontSize));
        }

        public float CalculateScaleForTextShaper(float fontSize) => CalculateScale(fontSize);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private float CalculateScale(float fontSize) => stbtt_ScaleForPixelHeight(_font, fontSize);

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_font != null)
                _font.Dispose();

            if (_dataPointer != IntPtr.Zero)
                Marshal.FreeHGlobal(_dataPointer);

            _disposed = true;
        }
    }
}
