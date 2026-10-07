using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     Parses the BBCode accepted by <see cref="SpriteTextPlusFormattable"/>.
    ///     Unknown tags are removed while their contents remain visible as plain text.
    /// </summary>
    internal static class FormattedTextMarkupParser
    {
        public static FormattedTextMarkupResult Parse(string markup)
        {
            markup = (markup ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            var result = new FormattedTextMarkupResult();
            ParseRange(markup, 0, markup.Length, result);
            return result;
        }

        private static void ParseRange(string source, int start, int end,
            FormattedTextMarkupResult result)
        {
            var index = start;

            while (index < end)
            {
                if (source[index] == '\\' && index + 1 < end && IsEscapable(source[index + 1]))
                {
                    result.Text.Append(source[index + 1]);
                    index += 2;
                    continue;
                }

                if (source[index] != '[' || !TryReadTag(source, index, end, out var tag))
                {
                    result.Text.Append(source[index]);
                    index++;
                    continue;
                }

                if (tag.IsClosing || tag.Kind == BbCodeKind.Unknown)
                {
                    index = tag.EndIndex;
                    continue;
                }

                if (TryApplyStandaloneTag(tag, result))
                {
                    index = tag.EndIndex;
                    continue;
                }

                if (!TryFindClosingTag(source, tag.EndIndex, end, tag.CanonicalName,
                        out var closingStart, out var closingEnd))
                {
                    index = tag.EndIndex;
                    continue;
                }

                ApplyContainerTag(source, tag, closingStart, result);
                index = closingEnd;
            }
        }

        private static bool TryApplyStandaloneTag(BbCodeTag tag,
            FormattedTextMarkupResult result)
        {
            switch (tag.Kind)
            {
                case BbCodeKind.LineBreak:
                    result.Text.Append('\n');
                    return true;
                case BbCodeKind.HorizontalRule:
                    result.Text.Append("────────");
                    return true;
                case BbCodeKind.ListMarker:
                    result.Text.Append("• ");
                    return true;
                default:
                    return false;
            }
        }

        private static void ApplyContainerTag(string source, BbCodeTag tag, int contentEnd,
            FormattedTextMarkupResult result)
        {
            var outputStart = result.Text.Length;

            switch (tag.Kind)
            {
                case BbCodeKind.Bold:
                {
                    var insertionIndex = result.BoldRanges.Count;
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    AddBoldRange(result, insertionIndex, outputStart);
                    break;
                }
                case BbCodeKind.Italic:
                {
                    var insertionIndex = result.ItalicRanges.Count;
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    AddItalicRange(result, insertionIndex, outputStart);
                    break;
                }
                case BbCodeKind.Underline:
                {
                    var insertionIndex = result.UnderlineRanges.Count;
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    AddUnderlineRange(result, insertionIndex, outputStart);
                    break;
                }
                case BbCodeKind.Color:
                {
                    var insertionIndex = result.ColorRanges.Count;
                    ParseRange(source, tag.EndIndex, contentEnd, result);

                    if (TryParseHexColor(tag.Value, out var color))
                    {
                        var length = result.Text.Length - outputStart;

                        if (length != 0)
                            result.ColorRanges.Insert(insertionIndex,
                                new TextColorRange(outputStart, length, color));
                    }

                    break;
                }
                case BbCodeKind.Size:
                {
                    var insertionIndex = result.FontSizeRanges.Count;
                    ParseRange(source, tag.EndIndex, contentEnd, result);

                    if (TryParseFontSize(tag.Value, out var fontSize))
                    {
                        var length = result.Text.Length - outputStart;

                        if (length != 0)
                            result.FontSizeRanges.Insert(insertionIndex,
                                new MarkupFontSizeRange(outputStart, length, fontSize));
                    }

                    break;
                }
                case BbCodeKind.Heading:
                {
                    var boldInsertionIndex = result.BoldRanges.Count;
                    var fontSizeInsertionIndex = result.FontSizeRanges.Count;
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    var length = result.Text.Length - outputStart;

                    if (length != 0)
                    {
                        result.BoldRanges.Insert(boldInsertionIndex,
                            new TextBoldRange(outputStart, length));
                        result.FontSizeRanges.Insert(fontSizeInsertionIndex,
                            new MarkupFontSizeRange(outputStart, length, tag.HeadingLevel));
                    }

                    break;
                }
                case BbCodeKind.Url:
                    ApplyUrl(source, tag, contentEnd, result, outputStart);
                    break;
                case BbCodeKind.Image:
                    ApplyImage(source, tag.EndIndex, contentEnd, result);
                    break;
                case BbCodeKind.Timestamp:
                    ApplyTimestamp(source, tag.EndIndex, contentEnd, result);
                    break;
                case BbCodeKind.Code:
                    result.Text.Append(source, tag.EndIndex, contentEnd - tag.EndIndex);
                    break;
                case BbCodeKind.Quote:
                    result.Text.Append("› ");
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    break;
                case BbCodeKind.ListItem:
                    result.Text.Append("• ");
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    break;
                case BbCodeKind.List:
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    break;
                default:
                    ParseRange(source, tag.EndIndex, contentEnd, result);
                    break;
            }
        }

        private static void ApplyUrl(string source, BbCodeTag tag, int contentEnd,
            FormattedTextMarkupResult result, int outputStart)
        {
            ParseRange(source, tag.EndIndex, contentEnd, result);
            var target = string.IsNullOrWhiteSpace(tag.Value)
                ? StripMarkup(source, tag.EndIndex, contentEnd)
                : NormalizeTarget(tag.Value);
            var length = result.Text.Length - outputStart;

            if (length == 0 && target.Length != 0)
            {
                result.Text.Append(target);
                length = target.Length;
            }

            if (length != 0 && target.Length != 0 &&
                !target.StartsWith(SpriteTextPlusFormattable.TimestampLinkTargetPrefix,
                    StringComparison.OrdinalIgnoreCase))
                result.LinkRanges.Add(new TextLinkRange(outputStart, length, target));
        }

        private static void ApplyImage(string source, int contentStart, int contentEnd,
            FormattedTextMarkupResult result)
        {
            var target = StripMarkup(source, contentStart, contentEnd);

            if (target.Length == 0)
                return;

            var outputStart = result.Text.Length;
            result.Text.Append(target);

            if (!target.StartsWith(SpriteTextPlusFormattable.TimestampLinkTargetPrefix,
                    StringComparison.OrdinalIgnoreCase))
                result.LinkRanges.Add(new TextLinkRange(outputStart, target.Length, target));
        }

        private static void ApplyTimestamp(string source, int contentStart, int contentEnd,
            FormattedTextMarkupResult result)
        {
            var timestamp = StripMarkup(source, contentStart, contentEnd);

            if (timestamp.Length == 0)
                return;

            var outputStart = result.Text.Length;
            result.Text.Append(timestamp);

            if (IsMapTimestamp(timestamp))
                result.LinkRanges.Add(new TextLinkRange(outputStart, timestamp.Length,
                    SpriteTextPlusFormattable.TimestampLinkTargetPrefix + timestamp));
        }

        private static string StripMarkup(string source, int start, int end)
        {
            var stripped = new FormattedTextMarkupResult();
            ParseRange(source, start, end, stripped);
            return stripped.Text.ToString().Trim();
        }

        private static void AddBoldRange(FormattedTextMarkupResult result, int insertionIndex,
            int outputStart)
        {
            var length = result.Text.Length - outputStart;

            if (length != 0)
                result.BoldRanges.Insert(insertionIndex, new TextBoldRange(outputStart, length));
        }

        private static void AddItalicRange(FormattedTextMarkupResult result, int insertionIndex,
            int outputStart)
        {
            var length = result.Text.Length - outputStart;

            if (length != 0)
                result.ItalicRanges.Insert(insertionIndex,
                    new TextItalicRange(outputStart, length));
        }

        private static void AddUnderlineRange(FormattedTextMarkupResult result,
            int insertionIndex, int outputStart)
        {
            var length = result.Text.Length - outputStart;

            if (length != 0)
                result.UnderlineRanges.Insert(insertionIndex,
                    new TextUnderlineRange(outputStart, length));
        }

        private static bool TryFindClosingTag(string source, int start, int end,
            string canonicalName, out int closingStart, out int closingEnd)
        {
            var depth = 1;
            var index = start;

            while (index < end)
            {
                if (source[index] != '[' || IsEscaped(source, index) ||
                    !TryReadTag(source, index, end, out var tag))
                {
                    index++;
                    continue;
                }

                if (tag.CanonicalName.Equals(canonicalName, StringComparison.OrdinalIgnoreCase))
                {
                    if (tag.IsClosing)
                        depth--;
                    else if (!IsStandalone(tag.Kind))
                        depth++;

                    if (depth == 0)
                    {
                        closingStart = index;
                        closingEnd = tag.EndIndex;
                        return true;
                    }
                }

                index = tag.EndIndex;
            }

            closingStart = -1;
            closingEnd = -1;
            return false;
        }

        private static bool TryReadTag(string source, int start, int end, out BbCodeTag tag)
        {
            tag = default;

            if (source[start] != '[' || IsEscaped(source, start))
                return false;

            var bracketEnd = start + 1;

            while (bracketEnd < end && source[bracketEnd] != ']' &&
                   source[bracketEnd] != '\r' && source[bracketEnd] != '\n')
                bracketEnd++;

            if (bracketEnd >= end || source[bracketEnd] != ']')
                return false;

            var content = source.Substring(start + 1, bracketEnd - start - 1).Trim();

            if (content.Length == 0)
                return false;

            var isClosing = content[0] == '/';

            if (isClosing)
                content = content.Substring(1).TrimStart();

            if (content.EndsWith("/", StringComparison.Ordinal))
                content = content.Substring(0, content.Length - 1).TrimEnd();

            var separator = content.IndexOf('=');
            var name = (separator == -1 ? content : content.Substring(0, separator)).Trim();
            var value = separator == -1 ? string.Empty : content.Substring(separator + 1).Trim();

            if (!IsValidTagName(name))
                return false;

            value = TrimQuotes(value);
            var kind = GetTagKind(name, out var canonicalName, out var headingLevel);
            tag = new BbCodeTag(kind, canonicalName, value, headingLevel, isClosing,
                bracketEnd + 1);
            return true;
        }

        private static BbCodeKind GetTagKind(string name, out string canonicalName,
            out int headingLevel)
        {
            canonicalName = name.ToLowerInvariant();
            headingLevel = 0;

            switch (canonicalName)
            {
                case "b":
                case "strong":
                    canonicalName = "b";
                    return BbCodeKind.Bold;
                case "i":
                case "em":
                    canonicalName = "i";
                    return BbCodeKind.Italic;
                case "u":
                case "underline":
                    canonicalName = "u";
                    return BbCodeKind.Underline;
                case "color":
                case "colour":
                    canonicalName = "color";
                    return BbCodeKind.Color;
                case "size":
                    return BbCodeKind.Size;
                case "url":
                    return BbCodeKind.Url;
                case "img":
                    return BbCodeKind.Image;
                case "code":
                    return BbCodeKind.Code;
                case "timestamp":
                    return BbCodeKind.Timestamp;
                case "quote":
                    return BbCodeKind.Quote;
                case "list":
                    return BbCodeKind.List;
                case "li":
                    return BbCodeKind.ListItem;
                case "*":
                    return BbCodeKind.ListMarker;
                case "hr":
                    return BbCodeKind.HorizontalRule;
                case "br":
                    return BbCodeKind.LineBreak;
            }

            if (canonicalName.Length == 2 && canonicalName[0] == 'h' &&
                canonicalName[1] >= '1' && canonicalName[1] <= '6')
            {
                headingLevel = canonicalName[1] - '0';
                return BbCodeKind.Heading;
            }

            return BbCodeKind.Unknown;
        }

        private static bool IsStandalone(BbCodeKind kind) =>
            kind == BbCodeKind.LineBreak || kind == BbCodeKind.HorizontalRule ||
            kind == BbCodeKind.ListMarker;

        private static bool IsValidTagName(string name)
        {
            if (name == "*")
                return true;

            if (name.Length == 0)
                return false;

            for (var i = 0; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]))
                    return false;
            }

            return true;
        }

        private static string TrimQuotes(string value)
        {
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[value.Length - 1] == '"') ||
                 (value[0] == '\'' && value[value.Length - 1] == '\'')))
                return value.Substring(1, value.Length - 2);

            return value;
        }

        private static string NormalizeTarget(string target)
        {
            target = TrimQuotes(target.Trim());

            for (var i = 0; i < target.Length; i++)
            {
                if (!char.IsWhiteSpace(target[i]))
                    continue;

                return target.Substring(0, i);
            }

            return target;
        }

        private static bool TryParseFontSize(string value, out float fontSize) =>
            float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                out fontSize) && fontSize > 0 && !float.IsNaN(fontSize) &&
            !float.IsInfinity(fontSize);

        private static bool TryParseHexColor(string value, out Color color)
        {
            color = Color.White;

            if (value.Length < 2 || value[0] != '#')
                return false;

            var hex = value.Substring(1);

            if (hex.Length == 3 || hex.Length == 4)
            {
                var expanded = new StringBuilder(hex.Length * 2);

                for (var i = 0; i < hex.Length; i++)
                    expanded.Append(hex[i], 2);

                hex = expanded.ToString();
            }

            if (hex.Length != 6 && hex.Length != 8)
                return false;

            if (!byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out var red) ||
                !byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out var green) ||
                !byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out var blue))
                return false;

            var alpha = byte.MaxValue;

            if (hex.Length == 8 && !byte.TryParse(hex.Substring(6, 2),
                    NumberStyles.HexNumber, CultureInfo.InvariantCulture, out alpha))
                return false;

            color = new Color(red, green, blue, alpha);
            return true;
        }

        private static bool IsMapTimestamp(string value)
        {
            if (value.Length == 0)
                return false;

            var index = 0;

            while (index < value.Length)
            {
                var digitStart = index;

                while (index < value.Length && char.IsDigit(value[index]))
                    index++;

                if (digitStart == index)
                    return false;

                if (index == value.Length)
                    return true;

                if (value[index++] != '|' || index >= value.Length ||
                    !char.IsDigit(value[index]))
                    return false;

                index++;

                if (index == value.Length)
                    return true;

                if (value[index++] != ',')
                    return false;
            }

            return true;
        }

        private static bool IsEscaped(string source, int index)
        {
            var slashCount = 0;

            for (var i = index - 1; i >= 0 && source[i] == '\\'; i--)
                slashCount++;

            return slashCount % 2 != 0;
        }

        private static bool IsEscapable(char value) =>
            value == '\\' || value == '[' || value == ']';

        private enum BbCodeKind
        {
            Unknown,
            Bold,
            Italic,
            Underline,
            Color,
            Size,
            Url,
            Image,
            Code,
            Timestamp,
            Heading,
            Quote,
            List,
            ListItem,
            ListMarker,
            HorizontalRule,
            LineBreak
        }

        private readonly struct BbCodeTag
        {
            public BbCodeKind Kind { get; }
            public string CanonicalName { get; }
            public string Value { get; }
            public int HeadingLevel { get; }
            public bool IsClosing { get; }
            public int EndIndex { get; }

            public BbCodeTag(BbCodeKind kind, string canonicalName, string value,
                int headingLevel, bool isClosing, int endIndex)
            {
                Kind = kind;
                CanonicalName = canonicalName;
                Value = value;
                HeadingLevel = headingLevel;
                IsClosing = isClosing;
                EndIndex = endIndex;
            }
        }
    }

    internal readonly struct MarkupFontSizeRange
    {
        public int StartIndex { get; }
        public int Length { get; }
        public float FontSize { get; }
        public int HeadingLevel { get; }
        public bool IsHeading { get; }

        public MarkupFontSizeRange(int startIndex, int length, float fontSize)
        {
            StartIndex = startIndex;
            Length = length;
            FontSize = fontSize;
            HeadingLevel = 0;
            IsHeading = false;
        }

        public MarkupFontSizeRange(int startIndex, int length, int headingLevel)
        {
            StartIndex = startIndex;
            Length = length;
            FontSize = 0;
            HeadingLevel = headingLevel;
            IsHeading = true;
        }
    }

    internal sealed class FormattedTextMarkupResult
    {
        public StringBuilder Text { get; } = new StringBuilder();
        public List<TextBoldRange> BoldRanges { get; } = new List<TextBoldRange>();
        public List<TextItalicRange> ItalicRanges { get; } = new List<TextItalicRange>();
        public List<TextColorRange> ColorRanges { get; } = new List<TextColorRange>();
        public List<MarkupFontSizeRange> FontSizeRanges { get; } = new List<MarkupFontSizeRange>();
        public List<TextUnderlineRange> UnderlineRanges { get; } = new List<TextUnderlineRange>();
        public List<TextLinkRange> LinkRanges { get; } = new List<TextLinkRange>();
    }
}
