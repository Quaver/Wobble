using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     Resolves possibly overlapping underline ranges into contiguous, color-aware segments.
    /// </summary>
    internal static class TextUnderlineLayout
    {
        /// <summary>
        ///     Merges overlapping or touching underline ranges into contiguous decorated areas.
        /// </summary>
        public static List<TextUnderlineArea> BuildAreas(int textLength, IReadOnlyList<TextUnderlineRange> underlineRanges)
        {
            var ranges = new List<TextUnderlineArea>(underlineRanges.Count);

            for (var i = 0; i < underlineRanges.Count; i++)
            {
                var range = underlineRanges[i];
                var start = Math.Max(0, Math.Min(textLength, range.StartIndex));
                var end = Math.Max(start, Math.Min(textLength, range.StartIndex + range.Length));

                if (start < end)
                    ranges.Add(new TextUnderlineArea(start, end - start));
            }

            ranges.Sort((first, second) => first.StartIndex.CompareTo(second.StartIndex));
            var result = new List<TextUnderlineArea>(ranges.Count);

            for (var i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];

                if (result.Count == 0 || range.StartIndex > result[result.Count - 1].EndIndex)
                {
                    result.Add(range);
                    continue;
                }

                var previous = result[result.Count - 1];
                var end = Math.Max(previous.EndIndex, range.EndIndex);
                result[result.Count - 1] = new TextUnderlineArea(previous.StartIndex, end - previous.StartIndex);
            }

            return result;
        }

        /// <summary>
        ///     Builds non-overlapping underline segments. Later color ranges take precedence.
        /// </summary>
        public static List<TextUnderlineSegment> Build(int textLength, IReadOnlyList<TextUnderlineRange> underlineRanges, IReadOnlyList<TextColorRange> colorRanges)
        {
            var result = new List<TextUnderlineSegment>();

            if (textLength <= 0 || underlineRanges.Count == 0)
                return result;

            var boundaries = new List<int>();

            for (var i = 0; i < underlineRanges.Count; i++)
                AddRangeBoundaries(boundaries, textLength, underlineRanges[i].StartIndex, underlineRanges[i].Length);

            for (var i = 0; i < colorRanges.Count; i++)
                AddRangeBoundaries(boundaries, textLength, colorRanges[i].StartIndex, colorRanges[i].Length);

            boundaries.Sort();

            for (var i = boundaries.Count - 1; i > 0; i--)
            {
                if (boundaries[i] == boundaries[i - 1])
                    boundaries.RemoveAt(i);
            }

            for (var i = 0; i < boundaries.Count - 1; i++)
            {
                var start = boundaries[i];
                var end = boundaries[i + 1];

                if (start == end || !IsUnderlined(start, underlineRanges))
                    continue;

                var color = ResolveColor(start, colorRanges);

                if (result.Count != 0)
                {
                    var previous = result[result.Count - 1];

                    if (previous.EndIndex == start && previous.Color == color)
                    {
                        result[result.Count - 1] = new TextUnderlineSegment(previous.StartIndex, end - previous.StartIndex, color);
                        continue;
                    }
                }

                result.Add(new TextUnderlineSegment(start, end - start, color));
            }

            return result;
        }

        private static void AddRangeBoundaries(List<int> boundaries, int textLength, int startIndex, int length)
        {
            var start = Math.Max(0, Math.Min(textLength, startIndex));
            var end = Math.Max(start, Math.Min(textLength, startIndex + length));

            if (start == end)
                return;

            boundaries.Add(start);
            boundaries.Add(end);
        }

        private static bool IsUnderlined(int textIndex, IReadOnlyList<TextUnderlineRange> ranges)
        {
            for (var i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];

                if (textIndex >= range.StartIndex && textIndex < range.StartIndex + range.Length)
                    return true;
            }

            return false;
        }

        private static Color ResolveColor(int textIndex, IReadOnlyList<TextColorRange> ranges)
        {
            var color = Color.White;

            for (var i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];

                if (textIndex >= range.StartIndex && textIndex < range.StartIndex + range.Length)
                    color = range.Color;
            }

            return color;
        }
    }

    /// <summary>
    ///     One contiguous underlined area, independent of color boundaries within it.
    /// </summary>
    internal readonly struct TextUnderlineArea
    {
        public int StartIndex { get; }
        public int Length { get; }
        public int EndIndex => StartIndex + Length;

        public TextUnderlineArea(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }
    }

    /// <summary>
    ///     One contiguous underline segment with one effective text color.
    /// </summary>
    internal readonly struct TextUnderlineSegment
    {
        public int StartIndex { get; }
        public int Length { get; }
        public int EndIndex => StartIndex + Length;
        public Color Color { get; }

        public TextUnderlineSegment(int startIndex, int length, Color color)
        {
            StartIndex = startIndex;
            Length = length;
            Color = color;
        }
    }
}