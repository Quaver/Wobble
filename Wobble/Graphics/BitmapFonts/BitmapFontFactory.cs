using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Logging;
using Color = Microsoft.Xna.Framework.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Image = System.Drawing.Image;

namespace Wobble.Graphics.BitmapFonts
{
    /// <summary>
    ///     Static class that stores custom fonts and creates BMP Texture2Ds
    ///
    ///     Useful Resources:
    ///         - https://stackoverflow.com/questions/6311545/c-sharp-write-text-on-bitmap
    ///         - https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-create-a-private-font-collection
    /// </summary>
    public static class BitmapFontFactory
    {
        /// <summary>
        ///    Stores all of the available custom fonts.
        /// </summary>
        public static Dictionary<string, FontStore> CustomFonts { get; } = new Dictionary<string, FontStore>();

        /// <summary>
        ///     Adds a font from memory/resource store.
        /// </summary>
        /// <param name="name">The name of the font to use for the store.</param>
        /// <param name="fontBytes"></param>
        public static void AddFont(string name, byte[] fontBytes)
        {
            var fontData = Marshal.AllocCoTaskMem(fontBytes.Length);
            Marshal.Copy(fontBytes, 0, fontData, fontBytes.Length);

            var fontCollection = new PrivateFontCollection();
            fontCollection.AddMemoryFont(fontData, fontBytes.Length);

            CustomFonts.Add(name, new FontStore(name, fontCollection.Families[0]));

            fontCollection.Dispose();
        }

        /// <summary>
        ///     Adds a font from a file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        public static void AddFont(string name, string filePath)
        {
            var fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(filePath);

            CustomFonts.Add(name, new FontStore(name, fontCollection.Families[0]));

            fontCollection.Dispose();
        }

        ///  <summary>
        ///     Create a Texture2D from a bitmap font.
        ///  </summary>
        /// <param name="fontName"></param>
        /// <param name="text"></param>
        ///  <param name="fontSize"></param>
        ///  <param name="color"></param>
        /// <param name="textAlignment"></param>
        /// <param name="maxWidth"></param>
        ///  <returns></returns>
        internal static Texture2D Create(string fontName, string text, int fontSize, Color color, Alignment textAlignment, int maxWidth)
        {
            // Stores the size of the text & Texture2D
            SizeF textSize;

            var alignment = GetAlignment(textAlignment);

            var font = GetCustomFont(fontName, fontSize) ?? new Font(fontName, fontSize, FontStyle.Regular);

            // Here we're creating a "virtual" graphics instance to measure
            // the size of the text.
            using (var bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            using (var format = new StringFormat())
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PageUnit = GraphicsUnit.Pixel;

                format.Alignment = alignment;
                format.LineAlignment = alignment;

                // Measure the string
                textSize = g.MeasureString(text, font, maxWidth, format);
            }

            // Bitmaps must have non-zero size.
            textSize.Width = Math.Max(1, textSize.Width);
            textSize.Height = Math.Max(1, textSize.Height);

            // Create the actual bitmap using the size of the text.
            using (var bmp = new Bitmap((int) (textSize.Width + 0.5), (int) (textSize.Height + 0.5), PixelFormat.Format32bppArgb))
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(System.Drawing.Color.White))
            using (var format = new StringFormat())
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PageUnit = GraphicsUnit.Pixel;

                format.Alignment = alignment;
                format.LineAlignment = alignment;

                var rect = new RectangleF(0, 0, textSize.Width, textSize.Height);
                g.DrawString(text, font, brush, rect, format);

                // Flush all graphics changes to the bitmap
                g.Flush();

                // Dispose of the font.
                font.Dispose();

               // bmp.RawFormat = ImageFormat.Png;
                return AssetLoader.LoadTexture2D(ImageToByte2(bmp));
            }
        }

        /// <summary>
        ///     Gets a custom font by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static Font GetCustomFont(string name, int size)
        {
            if (!CustomFonts.ContainsKey(name))
                return null;

            if (CustomFonts[name].Family.IsStyleAvailable(FontStyle.Regular))
                return new Font(CustomFonts[name].Family, size, FontStyle.Regular);

            if (CustomFonts[name].Family.IsStyleAvailable(FontStyle.Bold))
                return new Font(CustomFonts[name].Family, size, FontStyle.Bold);

            if (CustomFonts[name].Family.IsStyleAvailable(FontStyle.Italic))
                return new Font(CustomFonts[name].Family, size, FontStyle.Italic);

            if (CustomFonts[name].Family.IsStyleAvailable(FontStyle.Strikeout))
                return new Font(CustomFonts[name].Family, size, FontStyle.Strikeout);

            if (CustomFonts[name].Family.IsStyleAvailable(FontStyle.Underline))
                return new Font(CustomFonts[name].Family, size, FontStyle.Underline);

            return null;
        }

        /// <summary>
        ///     Gets the StringAlignment from a normal Wobble Alignment.
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static StringAlignment GetAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.TopLeft:
                case Alignment.MidLeft:
                case Alignment.BotLeft:
                    return StringAlignment.Near;
                case Alignment.TopCenter:
                case Alignment.MidCenter:
                case Alignment.BotCenter:
                    return StringAlignment.Center;
                case Alignment.TopRight:
                case Alignment.MidRight:
                case Alignment.BotRight:
                    return StringAlignment.Far;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }

        /// <summary>
        ///     Converts an image to a byte array.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        /// <summary>
        ///     Disposes of all custom fonts.
        /// </summary>
        internal static void Dispose()
        {
            foreach (var font in CustomFonts)
                font.Value.Family.Dispose();
        }
    }
}