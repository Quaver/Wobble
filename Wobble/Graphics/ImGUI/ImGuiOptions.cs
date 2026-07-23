using System.Collections.Generic;
using Hexa.NET.ImGui;

namespace Wobble.Graphics.ImGUI
{
    public class ImGuiOptions
    {
        public List<ImGuiFont> Fonts { get; }

        public bool LoadDefaultFont { get; }

        public ImGuiOptions(List<ImGuiFont> fonts, bool loadDefaultFont = true)
        {
            Fonts = fonts;
            LoadDefaultFont = loadDefaultFont;
        }
    }

    public class ImGuiFont
    {
        public string Path { get; }

        public ImFontPtr Context { get; set; }

        public int Size { get; }

        public List<ImGuiFontFallback> Fallbacks { get; }

        public ImGuiFont(string path, int size = 15, List<ImGuiFontFallback> fallbacks = null)
        {
            Path = path;
            Size = size;
            Fallbacks = fallbacks ?? new List<ImGuiFontFallback>();
        }
    }

    public class ImGuiFontFallback
    {
        public string Path { get; }

        public int Index { get; }

        public ImGuiGlyphRanges GlyphRanges { get; }

        public ImGuiFontFallback(string path, int index = 0, ImGuiGlyphRanges glyphRanges = ImGuiGlyphRanges.Default)
        {
            Path = path;
            Index = index;
            GlyphRanges = glyphRanges;
        }
    }

    public enum ImGuiGlyphRanges
    {
        Default,
        ChineseFull,
        ChineseSimplifiedCommon,
        Japanese,
        Korean,
        Cyrillic,
        Greek,
        Thai,
        Vietnamese
    }
}
