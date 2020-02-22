using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImGuiNET;

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

        public ImGuiFont(string path, int size = 15)
        {
            Path = path;
            Size = size;
        }
    }
}