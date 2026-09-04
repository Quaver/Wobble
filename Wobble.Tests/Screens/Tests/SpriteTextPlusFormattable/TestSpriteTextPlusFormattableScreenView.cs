using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;
using FormattableText = Wobble.Graphics.Sprites.Text.SpriteTextPlusFormattable;

namespace Wobble.Tests.Screens.Tests.SpriteTextPlusFormattable
{
    public class TestSpriteTextPlusFormattableScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(15, 20, 27);
        private static readonly Color PanelColor = new Color(27, 36, 47);
        private static readonly Color MutedColor = new Color(150, 163, 181);
        private static readonly Color AccentColor = new Color(47, 198, 239);
        private static readonly Color SuccessColor = new Color(105, 230, 166);
        private static readonly Color FailureColor = new Color(255, 105, 120);

        private const float PanelWidth = 400;
        private const float PanelHeight = 580;

        private readonly WobbleFontStore font = FontManager.GetWobbleFont("inter-regular");
        private readonly WobbleFontStore boldFont = FontManager.GetWobbleFont("inter-bold");

        private FormattableText toggleSample;
        private SpriteTextPlus toggleStatus;
        private bool formattingEnabled = true;

        public TestSpriteTextPlusFormattableScreenView(Screen screen) : base(screen)
        {
            CreateHeader();

            var rangePanel = CreatePanel(55, "MARKUP AND RENDER PATHS");
            var wrapPanel = CreatePanel(483, "NESTING, WRAPPING, LINKS");
            var checksPanel = CreatePanel(911, "DETERMINISTIC CHECKS");

            CreateRangeSamples(rangePanel);
            CreateWrappingAndLinkSample(wrapPanel);
            CreateMutationSample(wrapPanel);
            CreateChecks(checksPanel);
        }

        private void CreateHeader()
        {
            CreateText(Container, "SPRITETEXTPLUSFORMATTABLE", 0, 22, 27, Color.White, true, Alignment.TopCenter);
            CreateText(Container, "Markdown-style bold plus tagged sizes, colors, underlines, and links share one text layout.", 0, 60, 15, MutedColor, false, Alignment.TopCenter);
        }

        private void CreateRangeSamples(Container panel)
        {
            CreateText(panel, "Green ticks mark cap bottom; underline is below.", 18, 51, 13, MutedColor);

            const string overlapMarkup =
                "Default **[u]#FFAE42[[size=36]BI#BE74FF[[size=52]GG[/size]]EST[/size]][/u]** default";
            var overlap = new FormattableText(font, boldFont, overlapMarkup, 20)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 18,
                Y = 77,
                Tint = Color.White
            };
            var capBottom = overlap.Y + overlap.CapTopOffset + overlap.CapHeight;
            AddBar(panel, 12, capBottom, 5, 1, SuccessColor);
            AddBar(panel, PanelWidth - 17, capBottom, 5, 1, SuccessColor);
            AddOutline(panel, 12, 72, PanelWidth - 24, overlap.Height + 10, AccentColor);

            CreateText(panel, "Cached", 18, 167, 13, MutedColor);
            var cached = CreateCacheComparisonSample(panel, 18, 190, true);
            AddOutline(panel, 12, 185, PanelWidth - 24, cached.Height + 10, AccentColor);

            CreateText(panel, "Uncached", 18, 282, 13, MutedColor);
            var uncached = CreateCacheComparisonSample(panel, 18, 305, false);
            AddOutline(panel, 12, 300, PanelWidth - 24, uncached.Height + 10, AccentColor);

            CreateText(panel, $"Bounds: cached {cached.Width:0.##} x {cached.Height:0.##}  |  uncached {uncached.Width:0.##} x {uncached.Height:0.##}", 18, 395, 12, MutedColor);
            CreateText(panel, "SMALL's underline stays near SMALL; LARGE must not push it down. Cached and uncached should match.", 18, 423, 13, MutedColor, false, Alignment.TopLeft, PanelWidth - 36);
        }

        private FormattableText CreateCacheComparisonSample(Container panel, float x, float y, bool cached)
        {
            const string markup =
                "same [u]#2FC6EF[[size=14]SMALL[/size]][/u] and **[u]#FFAE42[[size=38]LARGE[/size]][/u]** text";
            return new FormattableText(font, boldFont, markup, 20, cached)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = x,
                Y = y,
                Tint = Color.White
            };
        }

        private void CreateWrappingAndLinkSample(Container panel)
        {
            CreateText(panel, "The outline is the exact MaxWidth. No glyph should cross it.", 18, 51, 13, MutedColor);

            const string markup =
                "Mixed-size [u]#2FC6EF[[size=27]wrapping[/size]][/u] keeps every glyph inside this box. " +
                "Hover [DEFAULT LINK](default-link) and [**#FFAE42[[size=32]CUSTOM LINK[/size]]**](custom-link) to compare link colors.";
            var wrapped = new FormattableText(font, boldFont, markup, 18)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 30,
                Y = 82,
                MaxWidth = 340,
                Tint = Color.White,
                LinkColor = AccentColor
            };

            AddOutline(panel, 30, 82, 340, wrapped.Height, AccentColor);

            var linkStatus = CreateText(panel, "Link status: not clicked", 18, 92 + wrapped.Height, 13, MutedColor);
            wrapped.LinkClicked += (sender, args) =>
            {
                linkStatus.Text = $"Link status: clicked '{args.Link.Target}'";
                linkStatus.Tint = SuccessColor;
            };
        }

        private void CreateMutationSample(Container panel)
        {
            CreateText(panel, "Mutation test", 18, 357, 14, Color.White, true);

            toggleSample = new FormattableText(font, boldFont, string.Empty, 18)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 18,
                Y = 385,
                Tint = Color.White
            };
            ApplyToggleFormatting();

            var button = new RoundedButton((sender, args) => ToggleFormatting())
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(18, 453),
                Size = new ScalableVector2(185, 38),
                CornerRadius = 5,
                Tint = AccentColor
            };
            button.SetLabel(boldFont, "Toggle parsed markup", 13, Color.White);

            toggleStatus = CreateText(panel, "Markup applied", 217, 464, 13, SuccessColor);
        }

        private void ToggleFormatting()
        {
            formattingEnabled = !formattingEnabled;

            if (formattingEnabled)
            {
                ApplyToggleFormatting();
                toggleStatus.Text = "Markup applied";
                toggleStatus.Tint = SuccessColor;
            }
            else
            {
                toggleSample.Text = "Clear and reapply HIGHLIGHT";
                toggleStatus.Text = "Plain text";
                toggleStatus.Tint = MutedColor;
            }
        }

        private void ApplyToggleFormatting()
        {
            toggleSample.Text =
                "Clear and reapply **[u]#FFAE42[[size=34]HIGHLIGHT[/size]][/u]**";
        }

        private void CreateChecks(Container panel)
        {
            var checks = RunChecks();
            var y = 55f;

            for (var i = 0; i < checks.Count; i++)
            {
                var check = checks[i];
                CreateText(panel, (check.Passed ? "PASS  " : "FAIL  ") + check.Name, 18, y, 13, check.Passed ? SuccessColor : FailureColor, false, Alignment.TopLeft, PanelWidth - 36);
                y += 37;
            }

            var passed = checks.Count(x => x.Passed);
            CreateText(panel, $"{passed}/{checks.Count} CHECKS PASSED", 18, 400, 16, passed == checks.Count ? SuccessColor : FailureColor, true);
            CreateText(panel, "These checks exercise logical bounds and layout. Use the other panels to inspect raster alignment and click handling.", 18, 442, 13, MutedColor, false, Alignment.TopLeft, PanelWidth - 36);
        }

        private List<LayoutCheck> RunChecks()
        {
            const string value = "Default LARGE default";
            const string styledMarkup =
                "Default **[u]#FFAE42[[size=38]LARGE[/size]][/u]** [key], then [link](target) default";
            const string escapedMarkup =
                @"Escaped \**plain\**, \#FFAE42[color], and \[size=40\]literal\[/size\]";
            const string malformedMarkup = "Old [color=#FFAE42]color[/color], broken [size=30]size, and **bold";
            const string fallbackLinkMarkup =
                "Empty [](https://example.com) and image ![preview](https://example.com/image.png)";
            const string fallbackLinkText =
                "Empty https://example.com and image https://example.com/image.png";
            var rangeStart = value.IndexOf("LARGE", StringComparison.Ordinal);
            var parsed = new FormattableText(font, boldFont, styledMarkup, 20);
            var escaped = new FormattableText(font, boldFont, escapedMarkup, 20);
            var malformed = new FormattableText(font, boldFont, malformedMarkup, 20);
            var fallbackLinks = new FormattableText(font, boldFont, fallbackLinkMarkup, 20);
            var changing = new FormattableText(font, boldFont, value, 20);
            var cached = new FormattableText(font, boldFont, styledMarkup, 20, true);
            var uncached = new FormattableText(font, boldFont, styledMarkup, 20, false);
            var overlap = new FormattableText(font, value, 20);
            var resolved = new FormattableText(font, value, 20);
            var underlined = new FormattableText(font, value, 20);
            var wrapped = new FormattableText(font, boldFont, "A **[size=30]mixed-size[/size]** phrase should wrap without crossing the configured boundary.", 18);

            try
            {
                var originalWidth = changing.Width;
                var originalHeight = changing.Height;
                changing.Text = "Default **[size=38]LARGE[/size]** default";
                var largeRangeGrowsBounds = changing.Width > originalWidth && changing.Height > originalHeight;
                changing.Text = value;
                var resettingMarkupRestoresBounds = ApproximatelyEqual(originalWidth, changing.Width) &&
                                                    ApproximatelyEqual(originalHeight, changing.Height);

                overlap.SetTextFontSizeRanges(new[]
                {
                    new TextFontSizeRange(rangeStart, "LARGE".Length, 34),
                    new TextFontSizeRange(rangeStart + 1, 3, 48)
                });
                resolved.SetTextFontSizeRanges(new[]
                {
                    new TextFontSizeRange(rangeStart, 1, 34),
                    new TextFontSizeRange(rangeStart + 1, 3, 48),
                    new TextFontSizeRange(rangeStart + 4, 1, 34)
                });

                var underlineWidth = underlined.Width;
                var underlineHeight = underlined.Height;
                underlined.SetTextUnderlineRange(rangeStart, "LARGE".Length);

                wrapped.MaxWidth = 180;
                var wrappedLines = wrapped.Children.OfType<SpriteTextPlusLine>().ToList();

                return new List<LayoutCheck>
                {
                    new LayoutCheck("Markup and link fallbacks render expected text",
                        parsed.Text == "Default LARGE [key], then link default" &&
                        parsed.MarkupText == styledMarkup && fallbackLinks.Text == fallbackLinkText),
                    new LayoutCheck("Escaped markers remain literal", escaped.Text == "Escaped **plain**, #FFAE42[color], and [size=40]literal[/size]"),
                    new LayoutCheck("Old and malformed markup remain literal", malformed.Text == malformedMarkup),
                    new LayoutCheck("Markup mutation updates and restores bounds", largeRangeGrowsBounds && resettingMarkupRestoresBounds),
                    new LayoutCheck("Cached and uncached markup metrics agree", ApproximatelyEqual(cached.Width, uncached.Width) && ApproximatelyEqual(cached.Height, uncached.Height)),
                    new LayoutCheck("Programmatic overlapping range still works", ApproximatelyEqual(overlap.Width, resolved.Width) && ApproximatelyEqual(overlap.Height, resolved.Height)),
                    new LayoutCheck("Programmatic underline preserves bounds", ApproximatelyEqual(underlineWidth, underlined.Width) && ApproximatelyEqual(underlineHeight, underlined.Height)),
                    new LayoutCheck("Markup wrapping respects styled width", wrappedLines.Count > 1 && wrappedLines.All(x => x.LayoutWidth <= wrapped.MaxWidth + 0.01f)),
                    new LayoutCheck("Invalid programmatic ranges are rejected", InvalidFontSizeThrows(value) && InvalidUnderlineRangeThrows(value) && InvalidBoldRangeThrows(value))
                };
            }
            finally
            {
                parsed.Destroy();
                escaped.Destroy();
                malformed.Destroy();
                fallbackLinks.Destroy();
                changing.Destroy();
                cached.Destroy();
                uncached.Destroy();
                overlap.Destroy();
                resolved.Destroy();
                underlined.Destroy();
                wrapped.Destroy();
            }
        }

        private bool InvalidBoldRangeThrows(string value)
        {
            var text = new FormattableText(font, boldFont, value, 20);

            try
            {
                text.SetTextBoldRange(value.Length, 1);
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
            finally
            {
                text.Destroy();
            }
        }

        private bool InvalidFontSizeThrows(string value)
        {
            var text = new FormattableText(font, value, 20);

            try
            {
                text.SetTextFontSizeRange(0, 1, 0);
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
            finally
            {
                text.Destroy();
            }
        }

        private bool InvalidUnderlineRangeThrows(string value)
        {
            var text = new FormattableText(font, value, 20);

            try
            {
                text.SetTextUnderlineRange(value.Length, 1);
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
            finally
            {
                text.Destroy();
            }
        }

        private static bool ApproximatelyEqual(float first, float second) => Math.Abs(first - second) <= 1.01f;

        private Container CreatePanel(float x, string title)
        {
            var panel = new Container
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(x, 96),
                Size = new ScalableVector2(PanelWidth, PanelHeight)
            };

            new Sprite
            {
                Parent = panel,
                Image = WobbleAssets.WhiteBox,
                Size = panel.Size,
                Tint = PanelColor
            };

            CreateText(panel, title, 18, 18, 15, Color.White, true);
            return panel;
        }

        private SpriteTextPlus CreateText(Drawable parent, string value, float x, float y, int size, Color color, bool bold = false, Alignment alignment = Alignment.TopLeft, float? maxWidth = null)
        {
            return new SpriteTextPlus(bold ? boldFont : font, value, size)
            {
                Parent = parent,
                Alignment = alignment,
                X = x,
                Y = y,
                MaxWidth = maxWidth,
                Tint = color
            };
        }

        private void AddOutline(Drawable parent, float x, float y, float width, float height, Color color)
        {
            AddBar(parent, x, y, width, 1, color);
            AddBar(parent, x, y + height - 1, width, 1, color);
            AddBar(parent, x, y, 1, height, color);
            AddBar(parent, x + width - 1, y, 1, height, color);
        }

        private static void AddBar(Drawable parent, float x, float y, float width, float height, Color color) =>
            new Sprite
            {
                Parent = parent,
                Alignment = Alignment.TopLeft,
                Image = WobbleAssets.WhiteBox,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(width, height),
                Tint = color
            };

        public override void Update(GameTime gameTime) => Container.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();

        private readonly struct LayoutCheck
        {
            public string Name { get; }
            public bool Passed { get; }

            public LayoutCheck(string name, bool passed)
            {
                Name = name;
                Passed = passed;
            }
        }
    }
}
