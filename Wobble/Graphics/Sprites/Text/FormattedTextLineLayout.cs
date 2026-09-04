using System;
using System.Collections.Generic;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     Font- and size-aware layout for one displayed text line.
    /// </summary>
    internal sealed class FormattedTextLineLayout
    {
        /// <summary>
        ///     Contiguous text runs with a shared font and font size.
        /// </summary>
        public IReadOnlyList<FormattedTextRun> Runs { get; }

        /// <summary>
        ///     Width of all runs.
        /// </summary>
        public float Width { get; }

        /// <summary>
        ///     Height required by the tallest ascent and descent on the line.
        /// </summary>
        public float Height { get; }

        /// <summary>
        ///     Shared vertical draw-origin offset used to align every run. This is not a typographic baseline
        ///     coordinate and must not be used to position text decorations.
        /// </summary>
        public float DrawOffset { get; }

        /// <summary>
        ///     Height of the combined capital glyph area.
        /// </summary>
        public float CapHeight { get; }

        /// <summary>
        ///     Distance from the top of the line to the combined capital glyph area.
        /// </summary>
        public float CapTopOffset { get; }

        /// <summary>
        ///     Largest font size used by the line.
        /// </summary>
        public float MaxFontSize { get; }

        private FormattedTextLineLayout(IReadOnlyList<FormattedTextRun> runs, float width, float height, float drawOffset, float capHeight, float capTopOffset, float maxFontSize)
        {
            Runs = runs;
            Width = width;
            Height = height;
            DrawOffset = drawOffset;
            CapHeight = capHeight;
            CapTopOffset = capTopOffset;
            MaxFontSize = maxFontSize;
        }

        /// <summary>
        ///     Builds a line layout. Later size ranges take precedence when ranges overlap.
        /// </summary>
        public static FormattedTextLineLayout Build(WobbleFontStore font, WobbleFontStore boldFont,
            WobbleFontStore italicFont, WobbleFontStore boldItalicFont, string text,
            float defaultFontSize, IReadOnlyList<TextFontSizeRange> sizeRanges,
            IReadOnlyList<TextBoldRange> boldRanges, IReadOnlyList<TextItalicRange> italicRanges)
        {
            text = text ?? "";
            boldFont = boldFont ?? font;
            italicFont = italicFont ?? font;
            boldItalicFont = boldItalicFont ?? boldFont;
            var runs = CreateRuns(font, boldFont, italicFont, boldItalicFont, text,
                defaultFontSize, sizeRanges, boldRanges, italicRanges);

            if (runs.Count == 0)
            {
                font.FontSize = defaultFontSize;
                SpriteTextPlusLineRaw.GetVerticalLayout(font, out var emptyHeight, out var emptyDrawOffset, out var emptyCapHeight);

                return new FormattedTextLineLayout(runs, 0, emptyHeight, emptyDrawOffset, emptyCapHeight, (emptyHeight - emptyCapHeight) / 2f, defaultFontSize);
            }

            var width = 0f;
            var drawOffset = 0f;
            var descent = 0f;
            var maxFontSize = 0f;

            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                run.Font.FontSize = run.FontSize;

                var runWidth = run.Font.Store.MeasureString(run.Text).X;
                SpriteTextPlusLineRaw.GetVerticalLayout(run.Font, out var runHeight, out var runDrawOffset, out var runCapHeight);

                run.X = width;
                run.Width = runWidth;
                run.LayoutHeight = runHeight;
                run.DrawOffset = runDrawOffset;
                run.CapHeight = runCapHeight;

                width += runWidth;
                drawOffset = Math.Max(drawOffset, runDrawOffset);
                descent = Math.Max(descent, runHeight - runDrawOffset);
                maxFontSize = Math.Max(maxFontSize, run.FontSize);
            }

            var capTop = float.MaxValue;
            var capBottom = float.MinValue;

            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                var runCapTop = drawOffset - run.DrawOffset + (run.LayoutHeight - run.CapHeight) / 2f;
                capTop = Math.Min(capTop, runCapTop);
                capBottom = Math.Max(capBottom, runCapTop + run.CapHeight);
            }

            return new FormattedTextLineLayout(runs, width, drawOffset + descent, drawOffset, capBottom - capTop, capTop, maxFontSize);
        }

        /// <summary>
        ///     Measures from the beginning of the line to a UTF-16 character index.
        /// </summary>
        public float MeasureWidthToIndex(int textIndex)
        {
            if (textIndex <= 0)
                return 0;

            for (var i = 0; i < Runs.Count; i++)
            {
                var run = Runs[i];
                var runEnd = run.StartIndex + run.Length;

                if (textIndex >= runEnd)
                    continue;

                if (textIndex <= run.StartIndex)
                    return run.X;

                run.Font.FontSize = run.FontSize;
                return run.X + run.Font.Store.MeasureString(run.Text.Substring(0, textIndex - run.StartIndex)).X;
            }

            return Width;
        }

        /// <summary>
        ///     Gets the lowest capital-glyph edge and largest font size used inside one decorated range.
        /// </summary>
        public void GetUnderlineMetrics(int startIndex, int length, out float capBottom, out float maxFontSize)
        {
            var endIndex = startIndex + length;
            capBottom = 0;
            maxFontSize = 0;

            for (var i = 0; i < Runs.Count; i++)
            {
                var run = Runs[i];

                if (run.StartIndex >= endIndex || run.StartIndex + run.Length <= startIndex)
                    continue;

                var runCapTop = DrawOffset - run.DrawOffset + (run.LayoutHeight - run.CapHeight) / 2f;
                var runCapBottom = runCapTop + run.CapHeight;

                if (run.FontSize > maxFontSize)
                {
                    capBottom = runCapBottom;
                    maxFontSize = run.FontSize;
                }
                else if (run.FontSize == maxFontSize)
                    capBottom = Math.Max(capBottom, runCapBottom);
            }

            if (maxFontSize != 0)
                return;

            capBottom = CapTopOffset + CapHeight;
            maxFontSize = MaxFontSize;
        }

        /// <summary>
        ///     Splits text at font and size boundaries and merges adjacent ranges with the same effective style.
        /// </summary>
        private static List<FormattedTextRun> CreateRuns(WobbleFontStore font,
            WobbleFontStore boldFont, WobbleFontStore italicFont,
            WobbleFontStore boldItalicFont, string text, float defaultFontSize,
            IReadOnlyList<TextFontSizeRange> sizeRanges,
            IReadOnlyList<TextBoldRange> boldRanges,
            IReadOnlyList<TextItalicRange> italicRanges)
        {
            var result = new List<FormattedTextRun>();

            if (text.Length == 0)
                return result;

            var boundaries = new List<int> { 0, text.Length };

            for (var i = 0; i < sizeRanges.Count; i++)
            {
                AddBoundaries(boundaries, text.Length, sizeRanges[i].StartIndex, sizeRanges[i].Length);
            }

            for (var i = 0; i < boldRanges.Count; i++)
                AddBoundaries(boundaries, text.Length, boldRanges[i].StartIndex, boldRanges[i].Length);

            for (var i = 0; i < italicRanges.Count; i++)
                AddBoundaries(boundaries, text.Length, italicRanges[i].StartIndex,
                    italicRanges[i].Length);

            boundaries.Sort();

            for (var i = 0; i < boundaries.Count - 1; i++)
            {
                var start = boundaries[i];
                var end = boundaries[i + 1];

                if (start == end)
                    continue;

                var fontSize = defaultFontSize;

                for (var rangeIndex = 0; rangeIndex < sizeRanges.Count; rangeIndex++)
                {
                    var range = sizeRanges[rangeIndex];

                    if (start >= range.StartIndex && start < range.StartIndex + range.Length)
                        fontSize = range.FontSize;
                }

                var isBold = false;

                for (var rangeIndex = 0; rangeIndex < boldRanges.Count; rangeIndex++)
                {
                    var range = boldRanges[rangeIndex];

                    if (start >= range.StartIndex && start < range.StartIndex + range.Length)
                    {
                        isBold = true;
                        break;
                    }
                }

                var isItalic = false;

                for (var rangeIndex = 0; rangeIndex < italicRanges.Count; rangeIndex++)
                {
                    var range = italicRanges[rangeIndex];

                    if (start >= range.StartIndex && start < range.StartIndex + range.Length)
                    {
                        isItalic = true;
                        break;
                    }
                }

                var runFont = isBold
                    ? isItalic ? boldItalicFont : boldFont
                    : isItalic ? italicFont : font;

                if (result.Count != 0 && result[result.Count - 1].FontSize == fontSize &&
                    ReferenceEquals(result[result.Count - 1].Font, runFont))
                {
                    var previous = result[result.Count - 1];
                    previous.Length += end - start;
                    previous.Text += text.Substring(start, end - start);
                    continue;
                }

                result.Add(new FormattedTextRun(start, end - start, text.Substring(start, end - start), runFont, fontSize));
            }

            return result;
        }

        private static void AddBoundaries(List<int> boundaries, int textLength, int startIndex, int length)
        {
            var start = Math.Max(0, Math.Min(textLength, startIndex));
            var end = Math.Max(start, Math.Min(textLength, startIndex + length));

            if (!boundaries.Contains(start))
                boundaries.Add(start);

            if (!boundaries.Contains(end))
                boundaries.Add(end);
        }
    }

    /// <summary>
    ///     One contiguous font and size run in a formatted line.
    /// </summary>
    internal sealed class FormattedTextRun
    {
        public int StartIndex { get; }
        public int Length { get; set; }
        public string Text { get; set; }
        public WobbleFontStore Font { get; }
        public float FontSize { get; }
        public float X { get; set; }
        public float Width { get; set; }
        public float LayoutHeight { get; set; }
        public float DrawOffset { get; set; }
        public float CapHeight { get; set; }

        public FormattedTextRun(int startIndex, int length, string text, WobbleFontStore font, float fontSize)
        {
            StartIndex = startIndex;
            Length = length;
            Text = text;
            Font = font;
            FontSize = fontSize;
        }
    }
}