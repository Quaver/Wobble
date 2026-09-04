using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Primitives;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLineRaw : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get; }

        /// <summary>
        ///     The font used by bold ranges.
        /// </summary>
        public WobbleFontStore BoldFont { get; }

        /// <summary>
        ///     The font used by italic ranges.
        /// </summary>
        public WobbleFontStore ItalicFont { get; }

        /// <summary>
        ///     The font used by ranges that are both bold and italic.
        /// </summary>
        public WobbleFontStore BoldItalicFont { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private float _fontSize;
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                RefreshSize();
            }
        }

        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? "";
                RefreshSize();
            }
        }

        /// <summary>
        ///     Custom font sizes relative to this line.
        /// </summary>
        private IReadOnlyList<TextFontSizeRange> TextFontSizeRanges { get; set; } =
            Array.Empty<TextFontSizeRange>();

        /// <summary>
        ///     Bold ranges relative to this line.
        /// </summary>
        private IReadOnlyList<TextBoldRange> TextBoldRanges { get; set; } = Array.Empty<TextBoldRange>();

        /// <summary>
        ///     Italic ranges relative to this line.
        /// </summary>
        private IReadOnlyList<TextItalicRange> TextItalicRanges { get; set; } =
            Array.Empty<TextItalicRange>();

        /// <summary>
        ///     Custom colors relative to this line.
        /// </summary>
        private IReadOnlyList<TextColorRange> TextColorRanges { get; set; } = Array.Empty<TextColorRange>();

        /// <summary>
        ///     Underlined ranges relative to this line.
        /// </summary>
        private IReadOnlyList<TextUnderlineRange> TextUnderlineRanges { get; set; } =
            Array.Empty<TextUnderlineRange>();

        /// <summary>
        ///     Mixed-font or mixed-size layout, or null when the entire line uses the default style.
        /// </summary>
        private FormattedTextLineLayout FormattedLayout { get; set; }

        /// <summary>
        ///     The measured glyph width before render-target padding is applied.
        /// </summary>
        public float MeasuredWidth { get; private set; }

        /// <summary>
        ///     Optional per-glyph colors used when drawing this line.
        /// </summary>
        private Color[] GlyphColors { get; set; }

        /// <summary>
        ///     Content-independent line height used for layout.
        /// </summary>
        internal float LayoutHeight { get; private set; }

        /// <summary>
        ///     Height of a representative capital glyph.
        /// </summary>
        internal float CapHeight { get; private set; }

        /// <summary>
        ///     Distance from the logical line top to its combined capital glyph area.
        /// </summary>
        internal float CapTopOffset { get; private set; }

        /// <summary>
        ///     Padding around the rendered glyphs to prevent texture clipping.
        /// </summary>
        internal float RenderPadding { get; private set; }

        /// <summary>
        ///     Applies the font-size-specific vertical draw-origin offset.
        /// </summary>
        internal float VerticalDrawOffset { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="boldFont"></param>
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0,
            WobbleFontStore boldFont = null, WobbleFontStore italicFont = null,
            WobbleFontStore boldItalicFont = null)
        {
            Font = font;
            BoldFont = boldFont ?? font;
            ItalicFont = italicFont ?? font;
            BoldItalicFont = boldItalicFont ?? BoldFont;
            Text = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            if (FormattedLayout != null)
            {
                DrawFormattedText();
                DrawUnderlines(FormattedLayout);
                return;
            }

            Font.FontSize = FontSize;

            if (GlyphColors == null)
                Font.Store.DrawText(GameBase.Game.SpriteBatch, Text, AbsolutePosition, _color, scale: AbsoluteScale);
            else
                Font.Store.DrawText(GameBase.Game.SpriteBatch, Text, AbsolutePosition, GlyphColors, scale: AbsoluteScale);

            DrawUnderlines(null);
        }

        /// <summary>
        ///     Applies font sizes to ranges of this line.
        /// </summary>
        internal void SetTextFontSizeRanges(IReadOnlyList<TextFontSizeRange> ranges)
        {
            TextFontSizeRanges = new List<TextFontSizeRange>(ranges);
            RefreshSize();
        }

        /// <summary>
        ///     Clears all custom font sizes from this line.
        /// </summary>
        internal void ClearTextFontSizeRanges()
        {
            if (TextFontSizeRanges.Count == 0)
                return;

            TextFontSizeRanges = Array.Empty<TextFontSizeRange>();
            RefreshSize();
        }

        /// <summary>
        ///     Applies bold styling to ranges of this line.
        /// </summary>
        internal void SetTextBoldRanges(IReadOnlyList<TextBoldRange> ranges)
        {
            TextBoldRanges = new List<TextBoldRange>(ranges);
            RefreshSize();
        }

        /// <summary>
        ///     Clears all bold styling from this line.
        /// </summary>
        internal bool ClearTextBoldRanges()
        {
            if (TextBoldRanges.Count == 0)
                return false;

            TextBoldRanges = Array.Empty<TextBoldRange>();
            RefreshSize();
            return true;
        }

        /// <summary>
        ///     Applies italic styling to ranges of this line.
        /// </summary>
        internal void SetTextItalicRanges(IReadOnlyList<TextItalicRange> ranges)
        {
            TextItalicRanges = new List<TextItalicRange>(ranges);
            RefreshSize();
        }

        /// <summary>
        ///     Clears all italic styling from this line.
        /// </summary>
        internal bool ClearTextItalicRanges()
        {
            if (TextItalicRanges.Count == 0)
                return false;

            TextItalicRanges = Array.Empty<TextItalicRange>();
            RefreshSize();
            return true;
        }

        /// <summary>
        ///     Applies colors to ranges of this line.
        /// </summary>
        internal void SetTextColorRanges(IReadOnlyList<TextColorRange> ranges)
        {
            TextColorRanges = new List<TextColorRange>(ranges);
            RefreshGlyphColors();
        }

        /// <summary>
        ///     Clears all custom colors from this line.
        /// </summary>
        internal void ClearTextColorRanges()
        {
            if (TextColorRanges.Count == 0)
                return;

            TextColorRanges = Array.Empty<TextColorRange>();
            RefreshGlyphColors();
        }

        /// <summary>
        ///     Applies underlines to ranges of this line.
        /// </summary>
        internal void SetTextUnderlineRanges(IReadOnlyList<TextUnderlineRange> ranges) =>
            TextUnderlineRanges = new List<TextUnderlineRange>(ranges);

        /// <summary>
        ///     Clears all underlines from this line.
        /// </summary>
        internal bool ClearTextUnderlineRanges()
        {
            if (TextUnderlineRanges.Count == 0)
                return false;

            TextUnderlineRanges = Array.Empty<TextUnderlineRange>();
            return true;
        }

        /// <summary>
        ///     Measures from the beginning of this line to a UTF-16 character index.
        /// </summary>
        internal float MeasureWidthToIndex(int textIndex)
        {
            if (FormattedLayout != null)
                return FormattedLayout.MeasureWidthToIndex(textIndex);

            if (textIndex <= 0)
                return 0;

            Font.FontSize = FontSize;
            return textIndex >= Text.Length
                ? MeasuredWidth
                : Font.Store.MeasureString(Text.Substring(0, textIndex)).X;
        }

        private void RefreshSize()
        {
            if (TextFontSizeRanges.Count != 0 || TextBoldRanges.Count != 0 ||
                TextItalicRanges.Count != 0)
            {
                FormattedLayout = FormattedTextLineLayout.Build(Font, BoldFont, ItalicFont,
                    BoldItalicFont, Text, FontSize, TextFontSizeRanges, TextBoldRanges,
                    TextItalicRanges);
                MeasuredWidth = FormattedLayout.Width;
                RenderPadding = Math.Max(2f, FormattedLayout.MaxFontSize * 0.25f);
                LayoutHeight = FormattedLayout.Height;
                CapHeight = FormattedLayout.CapHeight;
                CapTopOffset = FormattedLayout.CapTopOffset;
                VerticalDrawOffset = FormattedLayout.DrawOffset;

                Y = RenderPadding / 2f;
                Size = new ScalableVector2(MeasuredWidth + RenderPadding, LayoutHeight + RenderPadding);
                RefreshGlyphColors();
                return;
            }

            FormattedLayout = null;
            Font.FontSize = FontSize;

            var (x, y) = Font.Store.MeasureString(Text);
            MeasuredWidth = x;
            RenderPadding = Math.Max(2f, FontSize * 0.25f);
            GetVerticalLayout(Font, out var fontLayoutHeight, out var drawOffset, out var capHeight);
            LayoutHeight = Math.Max(y, fontLayoutHeight);
            CapHeight = capHeight;
            CapTopOffset = (LayoutHeight - CapHeight) / 2f;
            VerticalDrawOffset = drawOffset;

            Y = RenderPadding / 2f + VerticalDrawOffset;
            Size = new ScalableVector2(x + RenderPadding, LayoutHeight + RenderPadding);
            RefreshGlyphColors();
        }

        /// <summary>
        ///     Draws each font and size run from a shared vertical draw origin.
        /// </summary>
        private void DrawFormattedText()
        {
            var position = AbsolutePosition;

            for (var i = 0; i < FormattedLayout.Runs.Count; i++)
            {
                var run = FormattedLayout.Runs[i];
                run.Font.FontSize = run.FontSize;

                var drawPosition = position + new Vector2(run.X, FormattedLayout.DrawOffset);
                var colors = CreateRunGlyphColors(run);

                if (colors == null)
                    run.Font.Store.DrawText(GameBase.Game.SpriteBatch, run.Text, drawPosition, _color, scale: AbsoluteScale);
                else
                    run.Font.Store.DrawText(GameBase.Game.SpriteBatch, run.Text, drawPosition, colors, scale: AbsoluteScale);
            }
        }

        /// <summary>
        ///     Maps line-relative color ranges onto one size run.
        /// </summary>
        private Color[] CreateRunGlyphColors(FormattedTextRun run)
        {
            if (TextColorRanges.Count == 0)
                return null;

            var runRanges = new List<TextColorRange>(TextColorRanges.Count);

            for (var i = 0; i < TextColorRanges.Count; i++)
            {
                var range = TextColorRanges[i];
                var start = Math.Max(range.StartIndex, run.StartIndex);
                var end = Math.Min(range.StartIndex + range.Length, run.StartIndex + run.Length);

                if (start < end)
                    runRanges.Add(new TextColorRange(start - run.StartIndex, end - start, range.Color));
            }

            return runRanges.Count == 0
                ? null
                : SpriteTextPlusLine.CreateGlyphColors(run.Font, run.FontSize, run.Text, runRanges);
        }

        /// <summary>
        ///     Refreshes the single-size glyph color array.
        /// </summary>
        private void RefreshGlyphColors()
        {
            GlyphColors = FormattedLayout == null && TextColorRanges.Count != 0
                ? SpriteTextPlusLine.CreateGlyphColors(Font, FontSize, Text, TextColorRanges)
                : null;
        }

        /// <summary>
        ///     Draws color-aware underline segments below the line's combined capital-glyph area.
        /// </summary>
        private void DrawUnderlines(FormattedTextLineLayout formattedLayout)
        {
            var segments = TextUnderlineLayout.Build(Text.Length, TextUnderlineRanges, TextColorRanges);
            var areas = TextUnderlineLayout.BuildAreas(Text.Length, TextUnderlineRanges);

            if (segments.Count == 0 || areas.Count == 0)
                return;

            var segmentIndex = 0;

            for (var areaIndex = 0; areaIndex < areas.Count; areaIndex++)
            {
                var area = areas[areaIndex];
                float capBottom;
                float maxFontSize;

                if (formattedLayout == null)
                {
                    capBottom = CapTopOffset + CapHeight - VerticalDrawOffset;
                    maxFontSize = FontSize;
                }
                else
                    formattedLayout.GetUnderlineMetrics(area.StartIndex, area.Length, out capBottom, out maxFontSize);

                var underlineY = capBottom + GetUnderlineOffset(maxFontSize);
                var thickness = GetUnderlineThickness(maxFontSize) * Math.Abs(AbsoluteScale.Y);

                while (segmentIndex < segments.Count && segments[segmentIndex].StartIndex < area.EndIndex)
                {
                    var segment = segments[segmentIndex++];
                    var startX = MeasureWidthToIndex(segment.StartIndex);
                    var endX = MeasureWidthToIndex(segment.EndIndex);

                    if (endX <= startX)
                        continue;

                    var start = AbsolutePosition + new Vector2(startX * AbsoluteScale.X, underlineY * AbsoluteScale.Y);
                    var end = AbsolutePosition + new Vector2(endX * AbsoluteScale.X, underlineY * AbsoluteScale.Y);
                    GameBase.Game.SpriteBatch.DrawLine(start, end, segment.Color, thickness);
                }
            }
        }

        /// <summary>
        ///     Separates underlines from the glyph area without letting very large ranges create excessive spacing.
        /// </summary>
        internal static float GetUnderlineOffset(float fontSize) =>
            Math.Max(1f, Math.Min(3f, fontSize * 0.08f));

        /// <summary>
        ///     Keeps underlines legible while preserving a consistent weight across mixed font sizes.
        /// </summary>
        internal static float GetUnderlineThickness(float fontSize) =>
            Math.Max(1f, Math.Min(2f, fontSize * 0.06f));

        internal static void GetVerticalLayout(WobbleFontStore font, out float layoutHeight, out float drawOffset, out float capHeight)
        {
            // "H" measures cap height; "Hgj" adds descenders so every string shares centered bounds and a stable baseline.
            var capBounds = font.Store.TextBounds("H", Vector2.Zero);
            capHeight = capBounds.Y2 - capBounds.Y;

            if (capHeight <= 0)
            {
                layoutHeight = font.Store.LineHeight;
                drawOffset = 0;
                capHeight = layoutHeight;
                return;
            }

            var fullBounds = font.Store.TextBounds("Hgj", Vector2.Zero);
            var fullHeight = fullBounds.Y2 - fullBounds.Y;
            var extensionHeight = Math.Max(0, fullHeight - capHeight);

            layoutHeight = Math.Max(font.Store.LineHeight, capHeight + extensionHeight * 2f);
            drawOffset = (layoutHeight - capHeight) / 2f - capBounds.Y;
        }
    }
}
