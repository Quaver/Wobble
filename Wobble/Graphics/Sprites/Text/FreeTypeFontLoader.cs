using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FontStashSharp.Interfaces;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace Wobble.Graphics.Sprites.Text
{
    internal class FreeTypeFontLoader : IFontLoader
    {
        private readonly Dictionary<byte[], IndexedFontSettings> _settings = new Dictionary<byte[], IndexedFontSettings>();

        public void Register(byte[] data, int index, int weight, int implicitFontSizeReduction,
            bool enableTabularNumbers)
        {
            _settings[data] = new IndexedFontSettings(index, weight, implicitFontSizeReduction,
                enableTabularNumbers);
        }

        public IFontSource Load(byte[] data)
        {
            IndexedFontSettings settings;
            if (!_settings.TryGetValue(data, out settings))
                settings = new IndexedFontSettings(0, FontWeight.Regular, 0, false);

            return new FreeTypeFontSource(data, settings);
        }
    }

    internal struct IndexedFontSettings
    {
        public int Index { get; }

        public int Weight { get; }

        public int ImplicitFontSizeReduction { get; }

        public bool EnableTabularNumbers { get; }

        public IndexedFontSettings(int index, int weight, int implicitFontSizeReduction,
            bool enableTabularNumbers)
        {
            Index = index;
            Weight = weight;
            ImplicitFontSizeReduction = implicitFontSizeReduction;
            EnableTabularNumbers = enableTabularNumbers;
        }
    }

    internal unsafe class FreeTypeFontSource : IFontSource
    {
        private const uint WeightAxisTag = 0x77676874;
        private const uint GsubTableTag = 0x47535542;
        private const uint TabularFiguresFeatureTag = 0x746e756d;

        private readonly object _lock = new object();
        private readonly int _implicitFontSizeReduction;
        private readonly Dictionary<int, int> _tabularDigitGlyphs;
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
            _tabularDigitGlyphs = settings.EnableTabularNumbers
                ? LoadSingleSubstitutionFeature(TabularFiguresFeatureTag)
                : null;
            _ascent = _face->ascender;
            _descent = _face->descender;
            _height = _face->height;
            _implicitFontSizeReduction = settings.ImplicitFontSizeReduction;
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
                if (glyph == 0)
                    return null;

                if (_tabularDigitGlyphs != null && codepoint >= '0' && codepoint <= '9'
                    && _tabularDigitGlyphs.TryGetValue((int)glyph, out var tabularGlyph))
                    return tabularGlyph;

                return (int)glyph;
            }
        }

        public void GetGlyphMetrics(int glyphId, float fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
        {
            lock (_lock)
            {
                LoadGlyph(glyphId, fontSize);

                var glyph = _face->glyph;
                var metrics = glyph->metrics;

                advance = ToPixels(glyph->advance.x);
                x0 = FloorToPixels(metrics.horiBearingX);
                y0 = -CeilToPixels(metrics.horiBearingY);
                x1 = CeilToPixels(metrics.horiBearingX + metrics.width);
                y1 = -FloorToPixels(metrics.horiBearingY - metrics.height);
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

        private Dictionary<int, int> LoadSingleSubstitutionFeature(uint featureTag)
        {
            var gsub = LoadSfntTable(GsubTableTag);

            if (gsub == null || gsub.Length < 10)
                return null;

            var featureListOffset = ReadUInt16(gsub, 6);
            var lookupListOffset = ReadUInt16(gsub, 8);
            var lookupIndices = FindFeatureLookupIndices(gsub, featureListOffset, featureTag);

            if (lookupIndices == null || lookupIndices.Count == 0)
                return null;

            var digitGlyphs = GetDigitGlyphIds();
            var substitutions = new Dictionary<int, int>();

            foreach (var lookupIndex in lookupIndices)
                ReadSingleSubstitutionLookup(gsub, lookupListOffset, lookupIndex, digitGlyphs, substitutions);

            return substitutions.Count == 0 ? null : substitutions;
        }

        private byte[] LoadSfntTable(uint tag)
        {
            UIntPtr length = UIntPtr.Zero;

            if (FT_Load_Sfnt_Table(_face, tag, IntPtr.Zero, null, &length) != FT_Error.FT_Err_Ok
                || length == UIntPtr.Zero)
                return null;

            var data = new byte[(int)length.ToUInt64()];

            fixed (byte* buffer = data)
            {
                ThrowOnError(FT_Load_Sfnt_Table(_face, tag, IntPtr.Zero, buffer, &length),
                    $"Unable to load {TagToString(tag)} font table.");
            }

            return data;
        }

        private Dictionary<int, bool> GetDigitGlyphIds()
        {
            var glyphs = new Dictionary<int, bool>();

            for (var codepoint = '0'; codepoint <= '9'; codepoint++)
            {
                var glyph = FT_Get_Char_Index(_face, (UIntPtr)(uint)codepoint);

                if (glyph != 0)
                    glyphs[(int)glyph] = true;
            }

            return glyphs;
        }

        private static List<ushort> FindFeatureLookupIndices(byte[] table, int featureListOffset, uint featureTag)
        {
            if (!CanRead(table, featureListOffset, 2))
                return null;

            var featureCount = ReadUInt16(table, featureListOffset);

            for (var i = 0; i < featureCount; i++)
            {
                var recordOffset = featureListOffset + 2 + i * 6;

                if (!CanRead(table, recordOffset, 6) || ReadUInt32(table, recordOffset) != featureTag)
                    continue;

                var featureOffset = featureListOffset + ReadUInt16(table, recordOffset + 4);

                if (!CanRead(table, featureOffset, 4))
                    return null;

                var lookupCount = ReadUInt16(table, featureOffset + 2);

                if (!CanRead(table, featureOffset + 4, lookupCount * 2))
                    return null;

                var indices = new List<ushort>();

                for (var j = 0; j < lookupCount; j++)
                    indices.Add(ReadUInt16(table, featureOffset + 4 + j * 2));

                return indices;
            }

            return null;
        }

        private static void ReadSingleSubstitutionLookup(byte[] table, int lookupListOffset, int lookupIndex,
            Dictionary<int, bool> sourceGlyphs, Dictionary<int, int> substitutions)
        {
            if (!CanRead(table, lookupListOffset, 2))
                return;

            var lookupCount = ReadUInt16(table, lookupListOffset);

            if (lookupIndex < 0 || lookupIndex >= lookupCount)
                return;

            var lookupOffsetRecord = lookupListOffset + 2 + lookupIndex * 2;

            if (!CanRead(table, lookupOffsetRecord, 2))
                return;

            var lookupOffset = lookupListOffset + ReadUInt16(table, lookupOffsetRecord);

            if (!CanRead(table, lookupOffset, 6) || ReadUInt16(table, lookupOffset) != 1)
                return;

            var subtableCount = ReadUInt16(table, lookupOffset + 4);

            if (!CanRead(table, lookupOffset + 6, subtableCount * 2))
                return;

            for (var i = 0; i < subtableCount; i++)
            {
                var subtableOffset = lookupOffset + ReadUInt16(table, lookupOffset + 6 + i * 2);
                ReadSingleSubstitutionSubtable(table, subtableOffset, sourceGlyphs, substitutions);
            }
        }

        private static void ReadSingleSubstitutionSubtable(byte[] table, int subtableOffset,
            Dictionary<int, bool> sourceGlyphs, Dictionary<int, int> substitutions)
        {
            if (!CanRead(table, subtableOffset, 6))
                return;

            var format = ReadUInt16(table, subtableOffset);
            var coverageOffset = subtableOffset + ReadUInt16(table, subtableOffset + 2);
            var coveredGlyphs = ReadCoverage(table, coverageOffset);

            if (coveredGlyphs == null)
                return;

            if (format == 1)
            {
                var delta = ReadInt16(table, subtableOffset + 4);

                foreach (var glyph in coveredGlyphs)
                {
                    if (sourceGlyphs.ContainsKey(glyph))
                        substitutions[glyph] = (glyph + delta) & 0xffff;
                }

                return;
            }

            if (format != 2)
                return;

            var glyphCount = ReadUInt16(table, subtableOffset + 4);

            if (coveredGlyphs.Count != glyphCount || !CanRead(table, subtableOffset + 6, glyphCount * 2))
                return;

            for (var i = 0; i < glyphCount; i++)
            {
                var glyph = coveredGlyphs[i];

                if (sourceGlyphs.ContainsKey(glyph))
                    substitutions[glyph] = ReadUInt16(table, subtableOffset + 6 + i * 2);
            }
        }

        private static List<int> ReadCoverage(byte[] table, int offset)
        {
            if (!CanRead(table, offset, 4))
                return null;

            var format = ReadUInt16(table, offset);
            var glyphs = new List<int>();

            if (format == 1)
            {
                var glyphCount = ReadUInt16(table, offset + 2);

                if (!CanRead(table, offset + 4, glyphCount * 2))
                    return null;

                for (var i = 0; i < glyphCount; i++)
                    glyphs.Add(ReadUInt16(table, offset + 4 + i * 2));

                return glyphs;
            }

            if (format != 2)
                return null;

            var rangeCount = ReadUInt16(table, offset + 2);

            if (!CanRead(table, offset + 4, rangeCount * 6))
                return null;

            for (var i = 0; i < rangeCount; i++)
            {
                var rangeOffset = offset + 4 + i * 6;
                var startGlyph = ReadUInt16(table, rangeOffset);
                var endGlyph = ReadUInt16(table, rangeOffset + 2);
                var startCoverageIndex = ReadUInt16(table, rangeOffset + 4);

                for (var glyph = startGlyph; glyph <= endGlyph; glyph++)
                {
                    var coverageIndex = startCoverageIndex + glyph - startGlyph;

                    while (glyphs.Count <= coverageIndex)
                        glyphs.Add(0);

                    glyphs[coverageIndex] = glyph;
                }
            }

            return glyphs;
        }

        private void LoadAndRenderGlyph(int glyphId, float fontSize)
        {
            LoadGlyph(glyphId, fontSize);
            ThrowOnError(FT_Render_Glyph(_face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL), "Unable to render glyph.");
        }

        private void LoadGlyph(int glyphId, float fontSize)
        {
            SetSize(fontSize);
            ThrowOnError(FT_Load_Glyph(_face, (uint)glyphId, FT_LOAD.FT_LOAD_NO_BITMAP), "Unable to load glyph.");
        }

        private void SetSize(float fontSize)
        {
            ThrowOnError(FT_Set_Pixel_Sizes(_face, 0, (uint)Math.Ceiling(ReduceFontSize(fontSize))), "Unable to set font size.");
        }

        private float CalculateScale(float fontSize)
        {
            return fontSize / (_ascent - _descent);
        }

        private float ReduceFontSize(float fontSize)
        {
            return Math.Max(1, fontSize - _implicitFontSizeReduction);
        }

        private bool HasFlag(FT_FACE_FLAG flag)
        {
            return (_face->face_flags & (IntPtr)(int)flag) != IntPtr.Zero;
        }

        private static int ToPixels(IntPtr value)
        {
            return (int)Math.Round(value.ToInt64() / 64f);
        }

        private static int FloorToPixels(IntPtr value)
        {
            return (int)Math.Floor(value.ToInt64() / 64f);
        }

        private static int CeilToPixels(IntPtr value)
        {
            return (int)Math.Ceiling(value.ToInt64() / 64f);
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

        private static bool CanRead(byte[] data, int offset, int length)
        {
            return offset >= 0 && length >= 0 && offset <= data.Length - length;
        }

        private static ushort ReadUInt16(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        private static short ReadInt16(byte[] data, int offset)
        {
            return unchecked((short)ReadUInt16(data, offset));
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
        }

        private static string TagToString(uint tag)
        {
            var bytes = new[]
            {
                (byte)(tag >> 24),
                (byte)(tag >> 16),
                (byte)(tag >> 8),
                (byte)tag
            };

            return Encoding.ASCII.GetString(bytes);
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

        [DllImport("freetype", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT_Error FT_Load_Sfnt_Table(FT_FaceRec_* face, uint tag, IntPtr offset, byte* buffer,
            UIntPtr* length);

        [StructLayout(LayoutKind.Sequential)]
        private struct FT_MM_Var_
        {
            public uint AxisCount;
            public uint DesignsCount;
            public uint NamedStylesCount;
            private uint _padding;
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
            private IntPtr _padding;
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
