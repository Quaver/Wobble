using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.UI.Buttons;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     A <see cref="SpriteTextPlus"/> that parses BBCode formatting and clickable ranges.
    ///     Supported tags include bold, italic, underline, color, size, headings, code, quotes,
    ///     lists, URLs, images, and timestamps. Tags can be nested and brackets can be escaped
    ///     with a backslash.
    /// </summary>
    public class SpriteTextPlusFormattable : SpriteTextPlus
    {
        /// <summary>
        ///     Prefix assigned to link targets produced by [timestamp] BBCode. Consumers can subscribe
        ///     to <see cref="LinkClicked"/> and decide how timestamps behave in their own context.
        /// </summary>
        public const string TimestampLinkTargetPrefix = "timestamp:";

        /// <summary>
        ///     Removes BBCode tags and returns the text that would be displayed by this drawable.
        ///     This is useful for plain-text previews that perform their own truncation.
        /// </summary>
        public static string StripFormatting(string markup) =>
            FormattedTextMarkupParser.Parse(markup).Text.ToString();

        /// <summary>
        ///     Whether this drawable refreshes when a separate formatting font store changes.
        /// </summary>
        private readonly bool _subscribesToFormattingFontChanges;

        private readonly HashSet<WobbleFontStore> _formattingFontSubscriptions =
            new HashSet<WobbleFontStore>();

        /// <summary>
        ///     Whether bold ranges should follow changes to <see cref="SpriteTextPlus.Font"/>.
        /// </summary>
        private bool _usesBaseFontForBold;

        private readonly bool _usesBaseFontForItalic;

        private readonly bool _usesBoldFontForBoldItalic;

        /// <summary>
        ///     Character ranges rendered with <see cref="BoldFont"/>.
        /// </summary>
        private readonly List<TextBoldRange> _textBoldRanges = new List<TextBoldRange>();

        /// <summary>
        ///     Character ranges rendered with <see cref="ItalicFont"/>.
        /// </summary>
        private readonly List<TextItalicRange> _textItalicRanges = new List<TextItalicRange>();

        /// <summary>
        ///     Character ranges with custom colors. Later ranges take precedence when ranges overlap.
        /// </summary>
        private readonly List<TextColorRange> _textColorRanges = new List<TextColorRange>();

        /// <summary>
        ///     Link colors followed by parsed and programmatic colors in precedence order.
        /// </summary>
        private readonly List<TextColorRange> _effectiveTextColorRanges = new List<TextColorRange>();

        /// <summary>
        ///     Whether the final color range is a temporary overlay over the parsed and programmatic ranges.
        /// </summary>
        private bool _hasTextColorOverlayRange;

        /// <summary>
        ///     Character ranges that are underlined using their effective text colors.
        /// </summary>
        private readonly List<TextUnderlineRange> _textUnderlineRanges = new List<TextUnderlineRange>();

        /// <summary>
        ///     Character ranges with custom font sizes. Later ranges take precedence when ranges overlap.
        /// </summary>
        private readonly List<TextFontSizeRange> _textFontSizeRanges = new List<TextFontSizeRange>();

        private readonly List<MarkupFontSizeRange> _markupFontSizeRanges =
            new List<MarkupFontSizeRange>();

        private bool UsesMarkupFontSizeRanges { get; set; }

        /// <summary>
        ///     Current size-aware wrapped layout, or null while the text uses one font size.
        /// </summary>
        private List<WrappedTextLine> FormattedLines { get; set; }

        /// <summary>
        ///     Current displayed lines used to map global formatting ranges without rebuilding wrapping.
        /// </summary>
        private IReadOnlyList<WrappedTextLine> DisplayedLines { get; set; } = Array.Empty<WrappedTextLine>();

        /// <summary>
        ///     Character ranges that act as links.
        /// </summary>
        private readonly List<TextLinkRange> _textLinkRanges = new List<TextLinkRange>();

        /// <summary>
        ///     Invisible buttons used to route input through Wobble's normal button ownership rules.
        /// </summary>
        private readonly List<TextLinkButton> _textLinkButtons = new List<TextLinkButton>();

        /// <summary>
        ///     Link hit target currently hovered by the pointer.
        /// </summary>
        private TextLinkButton HoveredTextLinkButton { get; set; }

        /// <summary>
        ///     Original source containing lightweight markup.
        /// </summary>
        public string MarkupText { get; private set; } = string.Empty;

        /// <summary>
        ///     Gets the rendered plain text and parses lightweight markup when assigned.
        /// </summary>
        public override string Text
        {
            get => base.Text;
            set => ApplyMarkup(value);
        }

        /// <summary>
        ///     Font used by <c>[b]bold[/b]</c> ranges. Defaults to <see cref="SpriteTextPlus.Font"/> when no
        ///     distinct bold font is supplied.
        /// </summary>
        private WobbleFontStore _boldFont;
        public WobbleFontStore BoldFont
        {
            get => _boldFont;
            set
            {
                var usesBaseFont = value == null;
                value = value ?? Font;

                if (ReferenceEquals(_boldFont, value))
                {
                    _usesBaseFontForBold = usesBaseFont;
                    RefreshFormattingFontSubscriptions();
                    return;
                }

                _boldFont = value;
                _usesBaseFontForBold = usesBaseFont;
                if (_usesBoldFontForBoldItalic)
                    _boldItalicFont = value;

                RefreshFormattingFontSubscriptions();
                RefreshText();
            }
        }

        /// <summary>
        ///     Font used by italic BBCode ranges. Defaults to <see cref="SpriteTextPlus.Font"/>.
        /// </summary>
        private WobbleFontStore _italicFont;
        public WobbleFontStore ItalicFont => _italicFont;

        /// <summary>
        ///     Font used by ranges that combine strong emphasis and emphasis. Defaults to
        ///     <see cref="BoldFont"/>.
        /// </summary>
        private WobbleFontStore _boldItalicFont;
        public WobbleFontStore BoldItalicFont => _boldItalicFont;

        /// <summary>
        ///     Default color applied to linked character ranges. Explicit color ranges take precedence.
        ///     Null preserves the surrounding text color.
        /// </summary>
        private Color? _linkColor;
        public Color? LinkColor
        {
            get => _linkColor;
            set
            {
                if (_linkColor == value)
                    return;

                _linkColor = value;
                RefreshEffectiveTextColorRanges();
                ApplyTextColorRanges();
                ApplyTextUnderlineRanges();
            }
        }

        /// <inheritdoc />
        public override WobbleFontStore Font
        {
            get => base.Font;
            set
            {
                if (ReferenceEquals(base.Font, value))
                    return;

                if (_usesBaseFontForBold)
                    _boldFont = value;
                if (_usesBaseFontForItalic)
                    _italicFont = value;
                if (_usesBoldFontForBoldItalic)
                    _boldItalicFont = _boldFont;

                base.Font = value;
                RefreshFormattingFontSubscriptions();
            }
        }

        /// <summary>
        ///     Invoked when one of the configured links is clicked.
        /// </summary>
        public event EventHandler<TextLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="cache"></param>
        /// <param name="subscribeToFontChanges"></param>
        public SpriteTextPlusFormattable(WobbleFontStore font, string text, int size = 0, bool cache = true, bool subscribeToFontChanges = true)
            : this(font, null, null, null, text, size, cache, subscribeToFontChanges)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="boldFont">The font for bold ranges, or null to follow <paramref name="font"/>.</param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="cache"></param>
        /// <param name="subscribeToFontChanges"></param>
        public SpriteTextPlusFormattable(WobbleFontStore font, WobbleFontStore boldFont, string text, int size = 0, bool cache = true, bool subscribeToFontChanges = true)
            : this(font, boldFont, null, null, text, size, cache, subscribeToFontChanges)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="boldFont"></param>
        /// <param name="italicFont"></param>
        /// <param name="boldItalicFont"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="cache"></param>
        /// <param name="subscribeToFontChanges"></param>
        public SpriteTextPlusFormattable(WobbleFontStore font, WobbleFontStore boldFont,
            WobbleFontStore italicFont, WobbleFontStore boldItalicFont, string text, int size = 0,
            bool cache = true, bool subscribeToFontChanges = true)
            : base(font, string.Empty, size, cache, subscribeToFontChanges)
        {
            _subscribesToFormattingFontChanges = subscribeToFontChanges;
            _boldFont = boldFont ?? font;
            _usesBaseFontForBold = boldFont == null;
            _italicFont = italicFont ?? font;
            _usesBaseFontForItalic = italicFont == null;
            _boldItalicFont = boldItalicFont ?? _boldFont;
            _usesBoldFontForBoldItalic = boldItalicFont == null;
            RefreshFormattingFontSubscriptions();

            ApplyMarkup(text);
        }

        /// <summary>
        ///     Parses markup, replaces all parsed formatting ranges, and refreshes the rendered plain text.
        /// </summary>
        private void ApplyMarkup(string markup)
        {
            MarkupText = markup ?? string.Empty;
            var parsed = FormattedTextMarkupParser.Parse(MarkupText);

            _textBoldRanges.Clear();
            _textBoldRanges.AddRange(parsed.BoldRanges);
            _textItalicRanges.Clear();
            _textItalicRanges.AddRange(parsed.ItalicRanges);
            _hasTextColorOverlayRange = false;
            _textColorRanges.Clear();
            _textColorRanges.AddRange(parsed.ColorRanges);
            _markupFontSizeRanges.Clear();
            _markupFontSizeRanges.AddRange(parsed.FontSizeRanges);
            UsesMarkupFontSizeRanges = true;

            _textUnderlineRanges.Clear();
            _textUnderlineRanges.AddRange(parsed.UnderlineRanges);
            _textLinkRanges.Clear();
            _textLinkRanges.AddRange(parsed.LinkRanges);
            RefreshEffectiveTextColorRanges();

            var plainText = parsed.Text.ToString();

            if (base.Text != plainText)
                base.Text = plainText;
            else
                RefreshText();
        }

        private void RefreshFormattingFontSubscriptions()
        {
            foreach (var font in _formattingFontSubscriptions)
                font.Changed -= OnFormattingFontChanged;

            _formattingFontSubscriptions.Clear();
            if (!_subscribesToFormattingFontChanges)
                return;

            SubscribeToFormattingFont(_boldFont);
            SubscribeToFormattingFont(_italicFont);
            SubscribeToFormattingFont(_boldItalicFont);
        }

        private void SubscribeToFormattingFont(WobbleFontStore font)
        {
            if (font == null || ReferenceEquals(font, Font) || !_formattingFontSubscriptions.Add(font))
                return;

            font.Changed += OnFormattingFontChanged;
        }

        private void OnFormattingFontChanged(object sender, EventArgs e) => RefreshText();

        private static float GetHeadingScale(int level)
        {
            switch (level)
            {
                case 1:
                    return 1.8f;
                case 2:
                    return 1.5f;
                case 3:
                    return 1.3f;
                case 4:
                    return 1.15f;
                case 5:
                    return 1f;
                default:
                    return 0.9f;
            }
        }

        private void RebuildMarkupFontSizeRanges()
        {
            _textFontSizeRanges.Clear();
            for (var i = 0; i < _markupFontSizeRanges.Count; i++)
            {
                var range = _markupFontSizeRanges[i];
                var fontSize = range.FontSize;
                if (range.IsHeading)
                    fontSize = FontSize * GetHeadingScale(range.HeadingLevel);

                _textFontSizeRanges.Add(new TextFontSizeRange(range.StartIndex, range.Length, fontSize));
            }
        }

        private void StopUsingMarkupFontSizeRanges()
        {
            UsesMarkupFontSizeRanges = false;
            _markupFontSizeRanges.Clear();
        }

        /// <summary>
        ///     Applies a color to a character range while preserving the text's original layout and kerning.
        ///     The supplied color is multiplied by this sprite's <see cref="Tint"/> and <see cref="Alpha"/>.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="color"></param>
        public void SetTextColorRange(int startIndex, int length, Color color)
        {
            if (startIndex < 0 || startIndex > Text.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length < 0 || length > Text.Length - startIndex)
                throw new ArgumentOutOfRangeException(nameof(length));

            _hasTextColorOverlayRange = false;
            _textColorRanges.Clear();

            if (length != 0)
                _textColorRanges.Add(new TextColorRange(startIndex, length, color));

            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
        }

        /// <summary>
        ///     Applies colors to character ranges while preserving the text's original layout and kerning.
        ///     The supplied colors are multiplied by this sprite's <see cref="Tint"/> and <see cref="Alpha"/>.
        ///     Later ranges take precedence when ranges overlap.
        /// </summary>
        /// <param name="ranges"></param>
        public void SetTextColorRanges(IReadOnlyList<TextColorRange> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];

                if (range.StartIndex < 0 || range.StartIndex > Text.Length)
                    throw new ArgumentOutOfRangeException(nameof(ranges), $"The start index of range {i} is outside the text.");

                if (range.Length < 0 || range.Length > Text.Length - range.StartIndex)
                    throw new ArgumentOutOfRangeException(nameof(ranges), $"The length of range {i} is outside the text.");
            }

            _hasTextColorOverlayRange = false;
            _textColorRanges.Clear();

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length != 0)
                    _textColorRanges.Add(ranges[i]);
            }

            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
        }

        /// <summary>
        ///     Clears all custom character colors.
        /// </summary>
        public void ClearTextColorRanges()
        {
            if (_textColorRanges.Count == 0)
                return;

            _hasTextColorOverlayRange = false;
            _textColorRanges.Clear();
            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
        }

        /// <summary>
        ///     Applies a temporary color over a character range without replacing parsed or programmatic colors.
        ///     The overlay takes precedence over all other color ranges.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="color"></param>
        public void SetTextColorOverlayRange(int startIndex, int length, Color color)
        {
            if (startIndex < 0 || startIndex > Text.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length < 0 || length > Text.Length - startIndex)
                throw new ArgumentOutOfRangeException(nameof(length));

            RemoveTextColorOverlayRange();

            if (length != 0)
            {
                _textColorRanges.Add(new TextColorRange(startIndex, length, color));
                _hasTextColorOverlayRange = true;
            }

            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
        }

        /// <summary>
        ///     Clears the temporary color overlay while preserving parsed and programmatic colors.
        /// </summary>
        public void ClearTextColorOverlayRange()
        {
            if (!RemoveTextColorOverlayRange())
                return;

            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
        }

        /// <summary>
        ///     Removes the final color range when it represents the temporary overlay.
        /// </summary>
        /// <returns>Whether an overlay was removed.</returns>
        private bool RemoveTextColorOverlayRange()
        {
            if (!_hasTextColorOverlayRange)
                return false;

            _hasTextColorOverlayRange = false;

            if (_textColorRanges.Count != 0)
                _textColorRanges.RemoveAt(_textColorRanges.Count - 1);

            return true;
        }

        /// <summary>
        ///     Rebuilds color ranges with link colors first so explicit colors can override them.
        /// </summary>
        private void RefreshEffectiveTextColorRanges()
        {
            _effectiveTextColorRanges.Clear();

            if (_linkColor.HasValue)
            {
                for (var i = 0; i < _textLinkRanges.Count; i++)
                {
                    var link = _textLinkRanges[i];
                    _effectiveTextColorRanges.Add(new TextColorRange(link.StartIndex, link.Length,
                        _linkColor.Value));
                }
            }

            _effectiveTextColorRanges.AddRange(_textColorRanges);
        }

        /// <summary>
        ///     Renders a character range with <see cref="BoldFont"/>.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        public void SetTextBoldRange(int startIndex, int length)
        {
            var range = new TextBoldRange(startIndex, length);
            ValidateTextBoldRange(range, nameof(startIndex), nameof(length));

            _textBoldRanges.Clear();

            if (length != 0)
                _textBoldRanges.Add(range);

            RefreshText();
        }

        /// <summary>
        ///     Renders multiple character ranges with <see cref="BoldFont"/>. Overlapping ranges are merged by
        ///     the formatted layout.
        /// </summary>
        /// <param name="ranges"></param>
        public void SetTextBoldRanges(IReadOnlyList<TextBoldRange> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
                ValidateTextBoldRange(ranges[i], nameof(ranges), nameof(ranges));

            _textBoldRanges.Clear();

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length != 0)
                    _textBoldRanges.Add(ranges[i]);
            }

            RefreshText();
        }

        /// <summary>
        ///     Clears all bold character ranges.
        /// </summary>
        public void ClearTextBoldRanges()
        {
            if (_textBoldRanges.Count == 0)
                return;

            _textBoldRanges.Clear();
            RefreshText();
        }

        /// <summary>
        ///     Renders a character range with <see cref="ItalicFont"/>.
        /// </summary>
        public void SetTextItalicRange(int startIndex, int length)
        {
            var range = new TextItalicRange(startIndex, length);
            ValidateTextItalicRange(range, nameof(startIndex), nameof(length));

            _textItalicRanges.Clear();

            if (length != 0)
                _textItalicRanges.Add(range);

            RefreshText();
        }

        /// <summary>
        ///     Renders multiple character ranges with <see cref="ItalicFont"/>.
        /// </summary>
        public void SetTextItalicRanges(IReadOnlyList<TextItalicRange> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
                ValidateTextItalicRange(ranges[i], nameof(ranges), nameof(ranges));

            _textItalicRanges.Clear();

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length != 0)
                    _textItalicRanges.Add(ranges[i]);
            }

            RefreshText();
        }

        /// <summary>
        ///     Clears all italic character ranges.
        /// </summary>
        public void ClearTextItalicRanges()
        {
            if (_textItalicRanges.Count == 0)
                return;

            _textItalicRanges.Clear();
            RefreshText();
        }

        /// <summary>
        ///     Underlines a character range using its effective text color, including custom color ranges,
        ///     <see cref="Tint"/>, and <see cref="Alpha"/>.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        public void SetTextUnderlineRange(int startIndex, int length)
        {
            var range = new TextUnderlineRange(startIndex, length);
            ValidateTextUnderlineRange(range, nameof(startIndex), nameof(length));

            _textUnderlineRanges.Clear();

            if (length != 0)
                _textUnderlineRanges.Add(range);

            ApplyTextUnderlineRanges();
        }

        /// <summary>
        ///     Underlines multiple character ranges using their effective text colors. Overlapping ranges are merged.
        /// </summary>
        /// <param name="ranges"></param>
        public void SetTextUnderlineRanges(IReadOnlyList<TextUnderlineRange> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
                ValidateTextUnderlineRange(ranges[i], nameof(ranges), nameof(ranges));

            _textUnderlineRanges.Clear();

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length != 0)
                    _textUnderlineRanges.Add(ranges[i]);
            }

            ApplyTextUnderlineRanges();
        }

        /// <summary>
        ///     Clears all underlined character ranges.
        /// </summary>
        public void ClearTextUnderlineRanges()
        {
            if (_textUnderlineRanges.Count == 0)
                return;

            _textUnderlineRanges.Clear();
            ApplyTextUnderlineRanges();
        }

        /// <summary>
        ///     Applies a font size to a character range and refreshes wrapping and line metrics.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="fontSize"></param>
        public void SetTextFontSizeRange(int startIndex, int length, float fontSize)
        {
            var range = new TextFontSizeRange(startIndex, length, fontSize);
            ValidateTextFontSizeRange(range, nameof(startIndex), nameof(length), nameof(fontSize));

            StopUsingMarkupFontSizeRanges();
            _textFontSizeRanges.Clear();

            if (length != 0)
                _textFontSizeRanges.Add(range);

            RefreshText();
        }

        /// <summary>
        ///     Applies font sizes to character ranges. Later ranges take precedence when ranges overlap.
        /// </summary>
        /// <param name="ranges"></param>
        public void SetTextFontSizeRanges(IReadOnlyList<TextFontSizeRange> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
                ValidateTextFontSizeRange(ranges[i], nameof(ranges), nameof(ranges), nameof(ranges));

            StopUsingMarkupFontSizeRanges();
            _textFontSizeRanges.Clear();

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length != 0)
                    _textFontSizeRanges.Add(ranges[i]);
            }

            RefreshText();
        }

        /// <summary>
        ///     Clears all custom font sizes and restores <see cref="SpriteTextPlus.FontSize"/> for the full text.
        /// </summary>
        public void ClearTextFontSizeRanges()
        {
            if (_textFontSizeRanges.Count == 0)
                return;

            StopUsingMarkupFontSizeRanges();
            _textFontSizeRanges.Clear();
            RefreshText();
        }

        /// <summary>
        ///     Makes a character range clickable. The target is returned through <see cref="LinkClicked"/>.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="target"></param>
        public void SetTextLinkRange(int startIndex, int length, string target)
        {
            var range = new TextLinkRange(startIndex, length, target);
            ValidateTextLinkRange(range, nameof(startIndex), nameof(length));

            _textLinkRanges.Clear();

            if (length != 0)
                _textLinkRanges.Add(range);

            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
            RebuildTextLinkButtons();
        }

        /// <summary>
        ///     Makes multiple non-overlapping character ranges clickable. Link targets are returned through
        ///     <see cref="LinkClicked"/>.
        /// </summary>
        /// <param name="ranges"></param>
        public void SetTextLinkRanges(IReadOnlyList<TextLinkRange> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
                ValidateTextLinkRange(ranges[i], nameof(ranges), nameof(ranges));

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length == 0)
                    continue;

                for (var j = i + 1; j < ranges.Count; j++)
                {
                    if (ranges[j].Length != 0 && TextLinkRangesOverlap(ranges[i], ranges[j]))
                        throw new ArgumentException("Text link ranges cannot overlap.", nameof(ranges));
                }
            }

            _textLinkRanges.Clear();

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Length != 0)
                    _textLinkRanges.Add(ranges[i]);
            }

            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
            RebuildTextLinkButtons();
        }

        /// <summary>
        ///     Clears all clickable character ranges.
        /// </summary>
        public void ClearTextLinkRanges()
        {
            if (_textLinkRanges.Count == 0)
                return;

            _textLinkRanges.Clear();
            RefreshEffectiveTextColorRanges();
            ApplyTextColorRanges();
            RebuildTextLinkButtons();
        }

        /// <inheritdoc />
        protected override void OnTextLayoutRefreshed()
        {
            if (UsesMarkupFontSizeRanges)
                RebuildMarkupFontSizeRanges();

            ApplyTextFontSizeRanges();
            DisplayedLines = FormattedLines ?? (IsCached
                ? BuildWrappedLayout()
                : WrappedTextLayout.Build(Text, null, MeasureUncachedLineWidth));
            ApplyTextColorRanges();
            ApplyTextUnderlineRanges();
            RebuildTextLinkButtons();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var inputEnabled = IsVisibleInHierarchy();

            for (var i = 0; i < _textLinkButtons.Count; i++)
                _textLinkButtons[i].IsInteractionEnabled = inputEnabled;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Replaces the base layout with font- and size-aware lines when required.
        /// </summary>
        private void ApplyTextFontSizeRanges()
        {
            if (_textFontSizeRanges.Count == 0 && _textBoldRanges.Count == 0 &&
                _textItalicRanges.Count == 0)
            {
                FormattedLines = null;
                return;
            }

            if (IsCached)
                BuildCachedFormattedLayout();
            else
                BuildUncachedFormattedLayout();
        }

        /// <summary>
        ///     Builds cached line sprites using font- and size-aware wrapping and line metrics.
        /// </summary>
        private void BuildCachedFormattedLayout()
        {
            FormattedLines = BuildFormattedWrappedLayout(MaxWidth);

            for (var i = Children.Count - 1; i >= 0; i--)
                Children[i].Destroy();

            var lineSprites = new List<SpriteTextPlusLine>(FormattedLines.Count);
            var width = 0f;
            var height = 0f;

            for (var i = 0; i < FormattedLines.Count; i++)
            {
                var line = FormattedLines[i];
                var lineSprite = new SpriteTextPlusLine(Font, line.Text, FontSize, BoldFont,
                    ItalicFont, BoldItalicFont);
                var lineSizeRanges = GetLineFontSizeRanges(line, 1);
                var lineBoldRanges = GetLineBoldRanges(line);
                var lineItalicRanges = GetLineItalicRanges(line);

                if (lineSizeRanges.Count != 0)
                    lineSprite.SetTextFontSizeRanges(lineSizeRanges);

                if (lineBoldRanges.Count != 0)
                    lineSprite.SetTextBoldRanges(lineBoldRanges);

                if (lineItalicRanges.Count != 0)
                    lineSprite.SetTextItalicRanges(lineItalicRanges);

                lineSprite.Parent = this;
                lineSprite.Y = height + lineSprite.VerticalLayoutOffset;
                lineSprite.UsePreviousSpriteBatchOptions = true;
                lineSprite.Tint = Tint;
                lineSprite.Alpha = Alpha;
                lineSprites.Add(lineSprite);

                if (lineSprites.Count == 1)
                {
                    CapHeight = lineSprite.CapHeight;
                    CapTopOffset = lineSprite.CapTopOffset;
                }

                width = Math.Max(width, lineSprite.LayoutWidth);
                height += lineSprite.LayoutHeight;
            }

            Size = new ScalableVector2(width, height);

            for (var i = 0; i < lineSprites.Count; i++)
            {
                lineSprites[i].Alignment = Alignment.TopLeft;
                lineSprites[i].X = GetLineX(width, lineSprites[i].LayoutWidth);
            }
        }

        /// <summary>
        ///     Calculates uncached mixed-style bounds while preserving the base uncached hard-break behavior.
        /// </summary>
        private void BuildUncachedFormattedLayout()
        {
            FormattedLines = BuildFormattedWrappedLayout(null);
            var width = 0f;
            var height = 0f;

            for (var i = 0; i < FormattedLines.Count; i++)
            {
                var layout = BuildFormattedLineLayout(FormattedLines[i], 1);

                if (i == 0)
                {
                    CapHeight = layout.CapHeight;
                    CapTopOffset = layout.CapTopOffset;
                }

                width = Math.Max(width, layout.Width);
                height += layout.Height;
            }

            Size = new ScalableVector2(width, height);
        }

        /// <summary>
        ///     Builds wrapped lines using each range's effective font and font size.
        /// </summary>
        private List<WrappedTextLine> BuildFormattedWrappedLayout(float? maxWidth)
        {
            var renderScale = maxWidth == null ? 1 : SpriteTextPlusLine.GetRenderScale();

            return WrappedTextLayout.Build(Text, maxWidth, (line, startIndex) =>
            {
                var ranges = GetFontSizeRanges(startIndex, line.Length, renderScale);
                var boldRanges = GetBoldRanges(startIndex, line.Length);
                var italicRanges = GetItalicRanges(startIndex, line.Length);
                var layout = FormattedTextLineLayout.Build(Font, BoldFont, ItalicFont,
                    BoldItalicFont, line, FontSize * renderScale, ranges, boldRanges,
                    italicRanges);
                return maxWidth == null
                    ? layout.Width
                    : (float)Math.Ceiling(layout.Width) / renderScale;
            });
        }

        /// <summary>
        ///     Creates a font- and size-aware layout for one displayed line.
        /// </summary>
        private FormattedTextLineLayout BuildFormattedLineLayout(WrappedTextLine line, float scale) =>
            FormattedTextLineLayout.Build(Font, BoldFont, ItalicFont, BoldItalicFont, line.Text,
                FontSize * scale, GetLineFontSizeRanges(line, scale), GetLineBoldRanges(line),
                GetLineItalicRanges(line));

        /// <summary>
        ///     Maps global font-size ranges to one displayed line.
        /// </summary>
        private List<TextFontSizeRange> GetLineFontSizeRanges(WrappedTextLine line, float scale) =>
            GetFontSizeRanges(line.Start, line.Length, scale);

        /// <summary>
        ///     Maps global font-size ranges to an arbitrary text slice.
        /// </summary>
        private List<TextFontSizeRange> GetFontSizeRanges(int startIndex, int length, float scale)
        {
            var result = new List<TextFontSizeRange>(_textFontSizeRanges.Count);
            var endIndex = startIndex + length;

            for (var i = 0; i < _textFontSizeRanges.Count; i++)
            {
                var range = _textFontSizeRanges[i];
                var start = Math.Max(range.StartIndex, startIndex);
                var end = Math.Min(range.StartIndex + range.Length, endIndex);

                if (start < end)
                    result.Add(new TextFontSizeRange(start - startIndex, end - start, range.FontSize * scale));
            }

            return result;
        }

        /// <summary>
        ///     Maps global bold ranges to one displayed line.
        /// </summary>
        private List<TextBoldRange> GetLineBoldRanges(WrappedTextLine line) =>
            GetBoldRanges(line.Start, line.Length);

        /// <summary>
        ///     Maps global bold ranges to an arbitrary text slice.
        /// </summary>
        private List<TextBoldRange> GetBoldRanges(int startIndex, int length)
        {
            var result = new List<TextBoldRange>(_textBoldRanges.Count);
            var endIndex = startIndex + length;

            for (var i = 0; i < _textBoldRanges.Count; i++)
            {
                var range = _textBoldRanges[i];
                var start = Math.Max(range.StartIndex, startIndex);
                var end = Math.Min(range.StartIndex + range.Length, endIndex);

                if (start < end)
                    result.Add(new TextBoldRange(start - startIndex, end - start));
            }

            return result;
        }

        /// <summary>
        ///     Maps global italic ranges to one displayed line.
        /// </summary>
        private List<TextItalicRange> GetLineItalicRanges(WrappedTextLine line) =>
            GetItalicRanges(line.Start, line.Length);

        /// <summary>
        ///     Maps global italic ranges to an arbitrary text slice.
        /// </summary>
        private List<TextItalicRange> GetItalicRanges(int startIndex, int length)
        {
            var result = new List<TextItalicRange>(_textItalicRanges.Count);
            var endIndex = startIndex + length;

            for (var i = 0; i < _textItalicRanges.Count; i++)
            {
                var range = _textItalicRanges[i];
                var start = Math.Max(range.StartIndex, startIndex);
                var end = Math.Min(range.StartIndex + range.Length, endIndex);

                if (start < end)
                    result.Add(new TextItalicRange(start - startIndex, end - start));
            }

            return result;
        }

        /// <summary>
        ///     Gets the actual displayed lines for range mapping.
        /// </summary>
        private IReadOnlyList<WrappedTextLine> GetDisplayedLines() => DisplayedLines;

        /// <summary>
        ///     Maps the configured text color ranges onto each wrapped line.
        /// </summary>
        private void ApplyTextColorRanges()
        {
            if (!IsCached)
                return;

            var lines = GetDisplayedLines();
            var lineRanges = new List<TextColorRange>(_effectiveTextColorRanges.Count);
            var lineIndex = 0;

            for (var i = 0; i < Children.Count; i++)
            {
                if (!(Children[i] is SpriteTextPlusLine lineSprite))
                    continue;

                if (_effectiveTextColorRanges.Count == 0 || lineIndex >= lines.Count)
                {
                    lineSprite.ClearTextColorRanges();
                    lineIndex++;
                    continue;
                }

                var line = lines[lineIndex++];
                lineRanges.Clear();

                for (var rangeIndex = 0; rangeIndex < _effectiveTextColorRanges.Count; rangeIndex++)
                {
                    var range = _effectiveTextColorRanges[rangeIndex];
                    var rangeStart = Math.Max(range.StartIndex, line.Start);
                    var rangeEnd = Math.Min(range.StartIndex + range.Length, line.End);

                    if (rangeStart < rangeEnd)
                        lineRanges.Add(new TextColorRange(rangeStart - line.Start, rangeEnd - rangeStart, range.Color));
                }

                if (lineRanges.Count == 0)
                    lineSprite.ClearTextColorRanges();
                else
                    lineSprite.SetTextColorRanges(lineRanges);
            }
        }

        /// <summary>
        ///     Maps configured underlines onto each cached wrapped line.
        /// </summary>
        private void ApplyTextUnderlineRanges()
        {
            if (!IsCached)
                return;

            var lines = GetDisplayedLines();
            var lineIndex = 0;

            for (var i = 0; i < Children.Count; i++)
            {
                if (!(Children[i] is SpriteTextPlusLine lineSprite))
                    continue;

                if ((_textUnderlineRanges.Count == 0 && HoveredTextLinkButton == null) ||
                    lineIndex >= lines.Count)
                {
                    lineSprite.ClearTextUnderlineRanges();
                    lineIndex++;
                    continue;
                }

                var lineRanges = GetLineUnderlineRanges(lines[lineIndex++]);

                if (lineRanges.Count == 0)
                    lineSprite.ClearTextUnderlineRanges();
                else
                    lineSprite.SetTextUnderlineRanges(lineRanges);
            }
        }

        /// <summary>
        ///     Recreates transparent link hit targets from the current wrapped text layout.
        /// </summary>
        private void RebuildTextLinkButtons()
        {
            DestroyTextLinkButtons();

            if (_textLinkRanges.Count == 0)
                return;

            if (IsCached)
                BuildCachedTextLinkButtons();
            else
                BuildUncachedTextLinkButtons();
        }

        /// <summary>
        ///     Creates link hit targets positioned from the cached line sprites.
        /// </summary>
        private void BuildCachedTextLinkButtons()
        {
            var lines = GetDisplayedLines();
            var lineIndex = 0;

            for (var childIndex = 0; childIndex < Children.Count && lineIndex < lines.Count; childIndex++)
            {
                if (!(Children[childIndex] is SpriteTextPlusLine lineSprite))
                    continue;

                var line = lines[lineIndex++];
                var lineY = lineSprite.Y - lineSprite.VerticalLayoutOffset;

                AddTextLinkButtonsForLine(line, lineSprite.X, lineY, lineSprite.LayoutHeight, lineSprite.MeasureTextWidth);
            }
        }

        /// <summary>
        ///     Creates link hit targets for uncached text, which supports hard line breaks but not wrapping.
        /// </summary>
        private void BuildUncachedTextLinkButtons()
        {
            var lines = GetDisplayedLines();
            var lineY = 0f;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (_textFontSizeRanges.Count == 0 && _textBoldRanges.Count == 0 &&
                    _textItalicRanges.Count == 0)
                {
                    Font.FontSize = FontSize;
                    SpriteTextPlusLineRaw.GetVerticalLayout(Font, out var lineHeight, out _, out _);
                    AddTextLinkButtonsForLine(line, 0, lineY, lineHeight, textIndex => MeasureUncachedLineWidth(line.Text.Substring(0, textIndex)));
                    lineY += lineHeight;
                    continue;
                }

                var layout = BuildFormattedLineLayout(line, 1);
                AddTextLinkButtonsForLine(line, 0, lineY, layout.Height, layout.MeasureWidthToIndex);
                lineY += layout.Height;
            }
        }

        /// <summary>
        ///     Adds the portions of all links that intersect one displayed line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="lineX"></param>
        /// <param name="lineY"></param>
        /// <param name="lineHeight"></param>
        /// <param name="measureToIndex"></param>
        private void AddTextLinkButtonsForLine(WrappedTextLine line, float lineX, float lineY, float lineHeight, Func<int, float> measureToIndex)
        {
            for (var i = 0; i < _textLinkRanges.Count; i++)
            {
                var link = _textLinkRanges[i];
                var rangeStart = Math.Max(link.StartIndex, line.Start);
                var rangeEnd = Math.Min(link.StartIndex + link.Length, line.End);

                if (rangeStart >= rangeEnd)
                    continue;

                var localStart = rangeStart - line.Start;
                var localEnd = rangeEnd - line.Start;
                var startX = measureToIndex(localStart);
                var endX = measureToIndex(localEnd);

                if (endX <= startX || lineHeight <= 0)
                    continue;

                var button = new TextLinkButton(link, HandleTextLinkClicked)
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Position = new ScalableVector2(lineX + startX, lineY),
                    Size = new ScalableVector2(endX - startX, lineHeight),
                    Depth = -1,
                    IsInteractionEnabled = IsVisibleInHierarchy()
                };

                button.Hovered += HandleTextLinkHovered;
                button.LeftHover += HandleTextLinkLeftHover;

                _textLinkButtons.Add(button);
            }
        }

        /// <summary>
        ///     Destroys all current link hit targets.
        /// </summary>
        private void DestroyTextLinkButtons()
        {
            var hadHoveredLink = HoveredTextLinkButton != null;
            HoveredTextLinkButton = null;

            for (var i = _textLinkButtons.Count - 1; i >= 0; i--)
            {
                if (!_textLinkButtons[i].IsDisposed)
                    _textLinkButtons[i].Destroy();
            }

            _textLinkButtons.Clear();

            if (hadHoveredLink)
                ApplyTextUnderlineRanges();
        }

        /// <summary>
        ///     Relays a Wobble button click with the associated text link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleTextLinkClicked(object sender, EventArgs e)
        {
            if (sender is TextLinkButton button)
                LinkClicked?.Invoke(this, new TextLinkClickedEventArgs(button.Link));
        }

        /// <summary>
        ///     Underlines the full link represented by the hovered hit target.
        /// </summary>
        private void HandleTextLinkHovered(object sender, EventArgs e)
        {
            if (!(sender is TextLinkButton button) || ReferenceEquals(HoveredTextLinkButton, button))
                return;

            HoveredTextLinkButton = button;
            ApplyTextUnderlineRanges();
        }

        /// <summary>
        ///     Removes the temporary hover underline when the active hit target is left.
        /// </summary>
        private void HandleTextLinkLeftHover(object sender, EventArgs e)
        {
            if (!ReferenceEquals(HoveredTextLinkButton, sender))
                return;

            HoveredTextLinkButton = null;
            ApplyTextUnderlineRanges();
        }

        /// <summary>
        ///     Validates a bold range against the current text.
        /// </summary>
        private void ValidateTextBoldRange(TextBoldRange range, string startParameterName, string lengthParameterName)
        {
            if (range.StartIndex < 0 || range.StartIndex > Text.Length)
                throw new ArgumentOutOfRangeException(startParameterName);

            if (range.Length < 0 || range.Length > Text.Length - range.StartIndex)
                throw new ArgumentOutOfRangeException(lengthParameterName);
        }

        /// <summary>
        ///     Validates an italic range against the current text.
        /// </summary>
        private void ValidateTextItalicRange(TextItalicRange range, string startParameterName,
            string lengthParameterName)
        {
            if (range.StartIndex < 0 || range.StartIndex > Text.Length)
                throw new ArgumentOutOfRangeException(startParameterName);

            if (range.Length < 0 || range.Length > Text.Length - range.StartIndex)
                throw new ArgumentOutOfRangeException(lengthParameterName);
        }

        /// <summary>
        ///     Validates a font-size range against the current text.
        /// </summary>
        private void ValidateTextFontSizeRange(TextFontSizeRange range, string startParameterName, string lengthParameterName, string fontSizeParameterName)
        {
            if (range.StartIndex < 0 || range.StartIndex > Text.Length)
                throw new ArgumentOutOfRangeException(startParameterName);

            if (range.Length < 0 || range.Length > Text.Length - range.StartIndex)
                throw new ArgumentOutOfRangeException(lengthParameterName);

            if (range.FontSize <= 0 || float.IsNaN(range.FontSize) || float.IsInfinity(range.FontSize))
                throw new ArgumentOutOfRangeException(fontSizeParameterName);
        }

        /// <summary>
        ///     Validates an underline range against the current text.
        /// </summary>
        private void ValidateTextUnderlineRange(TextUnderlineRange range, string startParameterName, string lengthParameterName)
        {
            if (range.StartIndex < 0 || range.StartIndex > Text.Length)
                throw new ArgumentOutOfRangeException(startParameterName);

            if (range.Length < 0 || range.Length > Text.Length - range.StartIndex)
                throw new ArgumentOutOfRangeException(lengthParameterName);
        }

        /// <summary>
        ///     Validates a link against the current text.
        /// </summary>
        private void ValidateTextLinkRange(TextLinkRange range, string startParameterName, string lengthParameterName)
        {
            if (range.Target == null)
                throw new ArgumentException("Text link targets cannot be null.", startParameterName);

            if (range.StartIndex < 0 || range.StartIndex > Text.Length)
                throw new ArgumentOutOfRangeException(startParameterName);

            if (range.Length < 0 || range.Length > Text.Length - range.StartIndex)
                throw new ArgumentOutOfRangeException(lengthParameterName);
        }

        /// <summary>
        ///     Returns whether two non-empty link ranges overlap.
        /// </summary>
        private static bool TextLinkRangesOverlap(TextLinkRange first, TextLinkRange second) =>
            first.StartIndex < second.StartIndex + second.Length && second.StartIndex < first.StartIndex + first.Length;

        /// <summary>
        ///     Returns whether this drawable and all of its ancestors are currently visible.
        /// </summary>
        private bool IsVisibleInHierarchy()
        {
            for (Drawable drawable = this; drawable != null; drawable = drawable.Parent)
            {
                if (!drawable.Visible)
                    return false;
            }

            return !IsDisposed;
        }

        /// <summary>
        ///     Measures an uncached line using the same font size as the uncached draw path.
        /// </summary>
        private float MeasureUncachedLineWidth(string text)
        {
            Font.FontSize = FontSize;
            return Font.Store.MeasureString(text).X;
        }

        /// <inheritdoc />
        public override void DrawToSpriteBatch()
        {
            if (_textFontSizeRanges.Count == 0 && _textBoldRanges.Count == 0 &&
                _textItalicRanges.Count == 0)
            {
                DrawSingleSizeText();
                return;
            }

            if (IsCached || !Visible)
                return;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(false);
#endif

            var lines = GetDisplayedLines();
            var lineY = 0f;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var layout = BuildFormattedLineLayout(line, 1);
                var lineColors = GetLineColorRanges(line);

                for (var runIndex = 0; runIndex < layout.Runs.Count; runIndex++)
                {
                    var run = layout.Runs[runIndex];
                    var drawPosition = AbsolutePosition + new Vector2(run.X * AbsoluteScale.X, (lineY + layout.DrawOffset) * AbsoluteScale.Y);
                    var colors = CreateRunGlyphColors(run, lineColors);

                    run.Font.FontSize = run.FontSize;

                    if (colors == null)
                        run.Font.Store.DrawText(GameBase.Game.SpriteBatch, run.Text, drawPosition, Tint * Alpha, scale: AbsoluteScale);
                    else
                    {
                        for (var colorIndex = 0; colorIndex < colors.Length; colorIndex++)
                            colors[colorIndex] = MultiplyColors(colors[colorIndex], Tint) * Alpha;

                        run.Font.Store.DrawText(GameBase.Game.SpriteBatch, run.Text, drawPosition, colors, scale: AbsoluteScale);
                    }
                }

                DrawUnderlineSegments(line, lineY, layout, 0, 0, layout.MeasureWidthToIndex);

                lineY += layout.Height;
            }
        }

        /// <summary>
        ///     Uses the original single-size drawing path, optionally with per-glyph colors.
        /// </summary>
        private void DrawSingleSizeText()
        {
            if (IsCached || !Visible)
                return;

            if (_effectiveTextColorRanges.Count == 0)
                base.DrawToSpriteBatch();
            else
            {
                var colors = SpriteTextPlusLine.CreateGlyphColors(Font, FontSize, Text,
                    _effectiveTextColorRanges);

                if (colors == null)
                    base.DrawToSpriteBatch();
                else
                {

#if DEBUG
                    global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(false);
#endif

                    SetSize();
                    var drawPosition = AbsolutePosition;
                    drawPosition.Y += VerticalDrawOffset * AbsoluteScale.Y;

                    for (var i = 0; i < colors.Length; i++)
                        colors[i] = MultiplyColors(colors[i], Tint) * Alpha;

                    Font.Store.DrawText(GameBase.Game.SpriteBatch, Text, drawPosition, colors, scale: AbsoluteScale);
                }
            }

            DrawSingleSizeUnderlines();
        }

        /// <summary>
        ///     Draws underlines for uncached single-size text, including hard line breaks.
        /// </summary>
        private void DrawSingleSizeUnderlines()
        {
            if (_textUnderlineRanges.Count == 0 && HoveredTextLinkButton == null)
                return;

            var lines = GetDisplayedLines();
            Font.FontSize = FontSize;
            SpriteTextPlusLineRaw.GetVerticalLayout(Font, out var lineHeight, out _, out var capHeight);
            var capBottom = (lineHeight - capHeight) / 2f + capHeight;
            var lineY = 0f;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                DrawUnderlineSegments(line, lineY, null, capBottom, FontSize, textIndex => MeasureUncachedLineWidth(line.Text.Substring(0, textIndex)));
                lineY += lineHeight;
            }
        }

        /// <summary>
        ///     Maps global color ranges to one displayed line.
        /// </summary>
        private List<TextColorRange> GetLineColorRanges(WrappedTextLine line)
        {
            var result = new List<TextColorRange>(_effectiveTextColorRanges.Count);

            for (var i = 0; i < _effectiveTextColorRanges.Count; i++)
            {
                var range = _effectiveTextColorRanges[i];
                var start = Math.Max(range.StartIndex, line.Start);
                var end = Math.Min(range.StartIndex + range.Length, line.End);

                if (start < end)
                    result.Add(new TextColorRange(start - line.Start, end - start, range.Color));
            }

            return result;
        }

        /// <summary>
        ///     Maps global underline ranges to one displayed line.
        /// </summary>
        private List<TextUnderlineRange> GetLineUnderlineRanges(WrappedTextLine line)
        {
            var result = new List<TextUnderlineRange>(_textUnderlineRanges.Count + (HoveredTextLinkButton == null ? 0 : 1));

            for (var i = 0; i < _textUnderlineRanges.Count; i++)
            {
                var range = _textUnderlineRanges[i];
                var start = Math.Max(range.StartIndex, line.Start);
                var end = Math.Min(range.StartIndex + range.Length, line.End);

                if (start < end)
                    result.Add(new TextUnderlineRange(start - line.Start, end - start));
            }

            if (HoveredTextLinkButton != null)
            {
                var link = HoveredTextLinkButton.Link;
                var start = Math.Max(link.StartIndex, line.Start);
                var end = Math.Min(link.StartIndex + link.Length, line.End);

                if (start < end)
                    result.Add(new TextUnderlineRange(start - line.Start, end - start));
            }

            return result;
        }

        /// <summary>
        ///     Draws uncached underline segments with the same effective colors used by their glyphs.
        /// </summary>
        private void DrawUnderlineSegments(WrappedTextLine line, float lineY, FormattedTextLineLayout formattedLayout, float defaultCapBottom, float defaultFontSize, Func<int, float> measureToIndex)
        {
            var underlineRanges = GetLineUnderlineRanges(line);

            if (underlineRanges.Count == 0)
                return;

            var segments = TextUnderlineLayout.Build(line.Length, underlineRanges, GetLineColorRanges(line));
            var areas = TextUnderlineLayout.BuildAreas(line.Length, underlineRanges);
            var segmentIndex = 0;

            for (var areaIndex = 0; areaIndex < areas.Count; areaIndex++)
            {
                var area = areas[areaIndex];
                var capBottom = defaultCapBottom;
                var maxFontSize = defaultFontSize;

                formattedLayout?.GetUnderlineMetrics(area.StartIndex, area.Length, out capBottom, out maxFontSize);

                var underlineY = lineY + capBottom + SpriteTextPlusLineRaw.GetUnderlineOffset(maxFontSize);
                var thickness = SpriteTextPlusLineRaw.GetUnderlineThickness(maxFontSize) *
                                Math.Abs(AbsoluteScale.Y);

                while (segmentIndex < segments.Count && segments[segmentIndex].StartIndex < area.EndIndex)
                {
                    var segment = segments[segmentIndex++];
                    var startX = measureToIndex(segment.StartIndex);
                    var endX = measureToIndex(segment.EndIndex);

                    if (endX <= startX)
                        continue;

                    var color = MultiplyColors(segment.Color, Tint) * Alpha;
                    var start = AbsolutePosition + new Vector2(startX * AbsoluteScale.X, underlineY * AbsoluteScale.Y);
                    var end = AbsolutePosition + new Vector2(endX * AbsoluteScale.X, underlineY * AbsoluteScale.Y);
                    GameBase.Game.SpriteBatch.DrawLine(start, end, color, thickness);
                }
            }
        }

        /// <summary>
        ///     Maps line-relative colors to one mixed-size run.
        /// </summary>
        private Color[] CreateRunGlyphColors(FormattedTextRun run, IReadOnlyList<TextColorRange> lineColors)
        {
            if (lineColors.Count == 0)
                return null;

            var runColors = new List<TextColorRange>(lineColors.Count);

            for (var i = 0; i < lineColors.Count; i++)
            {
                var range = lineColors[i];
                var start = Math.Max(range.StartIndex, run.StartIndex);
                var end = Math.Min(range.StartIndex + range.Length, run.StartIndex + run.Length);

                if (start < end)
                    runColors.Add(new TextColorRange(start - run.StartIndex, end - start, range.Color));
            }

            return runColors.Count == 0
                ? null
                : SpriteTextPlusLine.CreateGlyphColors(run.Font, run.FontSize, run.Text, runColors);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            foreach (var font in _formattingFontSubscriptions)
                font.Changed -= OnFormattingFontChanged;

            _formattingFontSubscriptions.Clear();

            LinkClicked = null;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static Color MultiplyColors(Color first, Color second) => new Color(first.R * second.R / 255, first.G * second.G / 255, first.B * second.B / 255, first.A * second.A / 255);

        /// <summary>
        ///     A non-rendering button that gives each link segment normal Wobble input behavior.
        /// </summary>
        private sealed class TextLinkButton : Button
        {
            /// <summary>
            ///     Link represented by this hit target.
            /// </summary>
            public TextLinkRange Link { get; }

            /// <summary>
            /// </summary>
            /// <param name="link"></param>
            /// <param name="clickAction"></param>
            public TextLinkButton(TextLinkRange link, EventHandler clickAction) : base(clickAction) => Link = link;

            /// <inheritdoc />
            public override void DrawToSpriteBatch()
            {
            }
        }
    }
}
