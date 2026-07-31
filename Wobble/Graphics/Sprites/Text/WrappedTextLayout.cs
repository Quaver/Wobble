using System;
using System.Collections.Generic;
using System.Globalization;

namespace Wobble.Graphics.Sprites.Text
{
    internal readonly struct WrappedTextLine
    {
        public string Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int BreakLength { get; }
        public bool HasHardBreak { get; }

        public int End => Start + Length;
        public int NextStart => End + BreakLength;

        public WrappedTextLine(string text, int start, int length, int breakLength, bool hasHardBreak)
        {
            Text = text;
            Start = start;
            Length = length;
            BreakLength = breakLength;
            HasHardBreak = hasHardBreak;
        }
    }

    internal static class WrappedTextLayout
    {
        public static List<WrappedTextLine> Build(string text, float? maxWidth, Func<string, float> measure)
        {
            text = text ?? "";
            var result = new List<WrappedTextLine>();
            var logicalLineStart = 0;

            for (var i = 0; i <= text.Length; i++)
            {
                if (i != text.Length && text[i] != '\n')
                    continue;

                var hasHardBreak = i < text.Length;
                AddLogicalLine(result, text.Substring(logicalLineStart, i - logicalLineStart),
                    logicalLineStart, hasHardBreak, maxWidth, measure);
                logicalLineStart = i + 1;
            }

            return result;
        }

        private static void AddLogicalLine(List<WrappedTextLine> result, string line, int rawStart,
            bool hasHardBreak, float? maxWidth, Func<string, float> measure)
        {
            if (maxWidth == null || line.Length == 0 || measure(line) <= maxWidth)
            {
                result.Add(new WrappedTextLine(line, rawStart, line.Length, hasHardBreak ? 1 : 0, hasHardBreak));
                return;
            }

            var remaining = line;
            var remainingStart = rawStart;

            while (remaining.Length > 0 && measure(remaining) > maxWidth)
            {
                var spaces = new List<int>();
                for (var i = 0; i < remaining.Length; i++)
                {
                    if (char.IsWhiteSpace(remaining[i]))
                        spaces.Add(i);
                }

                var splitOnIndex = FindLastFittingIndex(spaces, remaining, maxWidth.Value, measure);
                int displayedLength;
                int consumedLength;

                if (splitOnIndex == -1)
                {
                    var lastIndex = spaces.Count > 0 ? spaces[0] : remaining.Length;
                    displayedLength = FindLastFittingCharacterIndex(remaining, lastIndex, maxWidth.Value, measure);
                    consumedLength = displayedLength;
                }
                else
                {
                    displayedLength = spaces[splitOnIndex];
                    consumedLength = displayedLength + 1;
                }

                result.Add(new WrappedTextLine(remaining.Substring(0, displayedLength), remainingStart,
                    displayedLength, consumedLength - displayedLength, false));
                remaining = remaining.Substring(consumedLength);
                remainingStart += consumedLength;
            }

            if (remaining.Length > 0)
            {
                result.Add(new WrappedTextLine(remaining, remainingStart, remaining.Length,
                    hasHardBreak ? 1 : 0, hasHardBreak));
            }
            else if (hasHardBreak && result.Count > 0)
            {
                var previous = result[result.Count - 1];
                result[result.Count - 1] = new WrappedTextLine(previous.Text, previous.Start, previous.Length,
                    previous.BreakLength + 1, true);
            }
        }

        private static int FindLastFittingIndex(IReadOnlyList<int> indexes, string line, float maxWidth,
            Func<string, float> measure)
        {
            var result = -1;
            var lo = 0;
            var hi = indexes.Count - 1;

            while (lo <= hi)
            {
                var mid = lo + (hi - lo) / 2;
                var index = indexes[mid];

                if (measure(line.Substring(0, index)) <= maxWidth)
                {
                    result = mid;
                    lo = mid + 1;
                }
                else
                    hi = mid - 1;
            }

            return result;
        }

        private static int FindLastFittingCharacterIndex(string line, int lastIndex, float maxWidth,
            Func<string, float> measure)
        {
            var starts = StringInfo.ParseCombiningCharacters(line);
            var boundaries = new List<int>(starts.Length + 1);
            for (var i = 1; i < starts.Length; i++)
            {
                if (starts[i] <= lastIndex)
                    boundaries.Add(starts[i]);
            }
            if (line.Length <= lastIndex)
                boundaries.Add(line.Length);

            if (boundaries.Count == 0)
                return line.Length;

            var result = boundaries[0];
            var lo = 1;
            var hi = boundaries.Count - 1;

            while (lo <= hi)
            {
                var mid = lo + (hi - lo) / 2;
                var boundary = boundaries[mid];

                if (measure(line.Substring(0, boundary)) <= maxWidth)
                {
                    result = boundary;
                    lo = mid + 1;
                }
                else
                    hi = mid - 1;
            }

            return result;
        }
    }
}
