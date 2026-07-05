using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace Wobble.Graphics.Sprites.Text
{
    internal class FreeTypeFontLoader : IFontLoader
    {
        private readonly Dictionary<byte[], IndexedFontSettings> _settings = new Dictionary<byte[], IndexedFontSettings>();

        public void Register(byte[] data, int index, int weight)
        {
            _settings[data] = new IndexedFontSettings(index, weight);
        }

        public IFontSource Load(byte[] data)
        {
            IndexedFontSettings settings;
            if (!_settings.TryGetValue(data, out settings))
                settings = new IndexedFontSettings(0, FontWeight.Regular);

            return new FreeTypeFontSource(data, settings);
        }
    }

    internal struct IndexedFontSettings
    {
        public int Index { get; }

        public int Weight { get; }

        public IndexedFontSettings(int index, int weight)
        {
            Index = index;
            Weight = weight;
        }
    }

    internal unsafe class FreeTypeFontSource : IFontSource
    {
        private const uint WeightAxisTag = 0x77676874;

        private readonly object _lock = new object();
        private readonly FreeTypeLibrary _library;
        private readonly GCHandle _dataHandle;
        private readonly FT_FaceRec_* _face;
        private readonly int _ascent;
        private readonly int _descent;
        private readonly int _height;
        private bool _disposed;

        public FreeTypeFontSource(byte[] data, IndexedFontSettings settings)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _library = new FreeTypeLibrary();
            _dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            FT_FaceRec_* face;
            ThrowOnError(FT_New_Memory_Face(_library.Native, (byte*)_dataHandle.AddrOfPinnedObject(), (IntPtr)data.Length, (IntPtr)settings.Index, &face),
                "Unable to initialize font data.");

            _face = face;
            SetWeight(settings.Weight);
            _ascent = _face->ascender;
            _descent = _face->descender;
            _height = _face->height;
        }

        ~FreeTypeFontSource()
        {
            Dispose(false);
        }

        public void GetMetricsForSize(float fontSize, out int ascent, out int descent, out int lineHeight)
        {
            var scale = CalculateScale(fontSize);
            ascent = (int)(_ascent * scale);
            descent = (int)(_descent * scale);
            lineHeight = (int)(_height * scale);
        }

        public int? GetGlyphId(int codepoint)
        {
            lock (_lock)
            {
                var glyph = FT_Get_Char_Index(_face, (UIntPtr)(uint)codepoint);
                return glyph == 0 ? (int?)null : (int)glyph;
            }
        }

        public void GetGlyphMetrics(int glyphId, float fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
        {
            lock (_lock)
            {
                LoadAndRenderGlyph(glyphId, fontSize);

                var glyph = _face->glyph;
                var bitmap = glyph->bitmap;

                advance = ToPixels(glyph->advance.x);
                x0 = glyph->bitmap_left;
                y0 = -glyph->bitmap_top;
                x1 = x0 + (int)bitmap.width;
                y1 = y0 + (int)bitmap.rows;
            }
        }

        public void RasterizeGlyphBitmap(int glyphId, float fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
        {
            lock (_lock)
            {
                LoadAndRenderGlyph(glyphId, fontSize);

                var bitmap = _face->glyph->bitmap;
                var source = bitmap.buffer;
                var sourcePitch = Math.Abs(bitmap.pitch);
                var width = Math.Min(outWidth, (int)bitmap.width);
                var height = Math.Min(outHeight, (int)bitmap.rows);

                for (var y = 0; y < height; y++)
                    Marshal.Copy((IntPtr)(source + y * sourcePitch), buffer, startIndex + y * outStride, width);
            }
        }

        public int GetGlyphKernAdvance(int previousGlyphId, int glyphId, float fontSize)
        {
            lock (_lock)
            {
                SetSize(fontSize);

                if (!HasFlag(FT_FACE_FLAG.FT_FACE_FLAG_KERNING))
                    return 0;

                FT_Vector_ kerning;
                ThrowOnError(FT_Get_Kerning(_face, (uint)previousGlyphId, (uint)glyphId, FT_Kerning_Mode_.FT_KERNING_DEFAULT, &kerning),
                    "Unable to get glyph kerning.");
                return ToPixels(kerning.x);
            }
        }

        public float CalculateScaleForTextShaper(float fontSize) => CalculateScale(fontSize);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void SetWeight(int weight)
        {
            if (weight == FontWeight.Regular || !HasFlag(FT_FACE_FLAG.FT_FACE_FLAG_MULTIPLE_MASTERS))
                return;

            FT_MM_Var_* variation;
            ThrowOnError(FT_Get_MM_Var(_face, &variation), "Unable to read font variation axes.");

            try
            {
                if (variation->AxisCount == 0)
                    return;

                var axes = variation->Axis;
                var coordinates = stackalloc IntPtr[(int)variation->AxisCount];
                var weightAxisFound = false;

                for (var i = 0; i < variation->AxisCount; i++)
                {
                    coordinates[i] = axes[i].Default;

                    if (axes[i].Tag != WeightAxisTag)
                        continue;

                    coordinates[i] = Clamp(ToFixed(weight), axes[i].Minimum, axes[i].Maximum);
                    weightAxisFound = true;
                }

                if (!weightAxisFound)
                    return;

                ThrowOnError(FT_Set_Var_Design_Coordinates(_face, variation->AxisCount, coordinates), "Unable to set font variation coordinates.");
            }
            finally
            {
                FT_Done_MM_Var(_library.Native, variation);
            }
        }

        private void LoadAndRenderGlyph(int glyphId, float fontSize)
        {
            SetSize(fontSize);
            ThrowOnError(FT_Load_Glyph(_face, (uint)glyphId, FT_LOAD.FT_LOAD_NO_BITMAP), "Unable to load glyph.");
            ThrowOnError(FT_Render_Glyph(_face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL), "Unable to render glyph.");
        }

        private void SetSize(float fontSize)
        {
            ThrowOnError(FT_Set_Pixel_Sizes(_face, 0, (uint)Math.Max(1, (int)Math.Ceiling(fontSize))), "Unable to set font size.");
        }

        private float CalculateScale(float fontSize)
        {
            return fontSize / (_ascent - _descent);
        }

        private bool HasFlag(FT_FACE_FLAG flag)
        {
            return (_face->face_flags & (IntPtr)(int)flag) != IntPtr.Zero;
        }

        private static int ToPixels(IntPtr value)
        {
            return (int)Math.Round(value.ToInt64() / 64f);
        }

        private static IntPtr ToFixed(int value)
        {
            return (IntPtr)((long)value << 16);
        }

        private static IntPtr Clamp(IntPtr value, IntPtr minimum, IntPtr maximum)
        {
            var localValue = value.ToInt64();
            var localMinimum = minimum.ToInt64();
            var localMaximum = maximum.ToInt64();

            return (IntPtr)Math.Max(localMinimum, Math.Min(localMaximum, localValue));
        }

        private static void ThrowOnError(FT_Error error, string message)
        {
            if (error != FT_Error.FT_Err_Ok)
                throw new InvalidOperationException($"{message} FreeType error: {error}");
        }

        [DllImport("freetype", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT_Error FT_Get_MM_Var(FT_FaceRec_* face, FT_MM_Var_** variation);

        [DllImport("freetype", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT_Error FT_Set_Var_Design_Coordinates(FT_FaceRec_* face, uint numCoords, IntPtr* coords);

        [DllImport("freetype", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FT_Done_MM_Var(FT_LibraryRec_* library, FT_MM_Var_* variation);

        [StructLayout(LayoutKind.Sequential)]
        private struct FT_MM_Var_
        {
            public uint AxisCount;
            public uint DesignsCount;
            public uint NamedStylesCount;
            public FT_Var_Axis_* Axis;
            public IntPtr NamedStyle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FT_Var_Axis_
        {
            public IntPtr Name;
            public IntPtr Minimum;
            public IntPtr Default;
            public IntPtr Maximum;
            public uint Tag;
            public uint StringId;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_face != null)
                FT_Done_Face(_face);

            if (_library != null)
                _library.Dispose();

            if (_dataHandle.IsAllocated)
                _dataHandle.Free();

            _disposed = true;
        }
    }
}
