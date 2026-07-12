using System;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Screens;
using MarqueeText = Wobble.Graphics.Sprites.Text.MarqueeSpriteText;

namespace Wobble.Tests.Screens.Tests.ButtonsGallery
{
    public class TestButtonsGalleryScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(17, 24, 32);
        private static readonly Color PanelColor = new Color(31, 41, 51);
        private static readonly Color AccentColor = new Color(15, 186, 229);
        private static readonly Color PurpleColor = new Color(117, 92, 222);
        private static readonly Color MutedColor = new Color(126, 139, 151);
        private static readonly Color DisabledColor = new Color(72, 82, 91);
        private static readonly Color FocusColor = new Color(255, 196, 87);
        private static readonly Color ScrollbarColor = new Color(86, 97, 107);

        private const int ContentHeight = 780;

        private ScrollContainer ScreenScrollContainer { get; }

        private Container ContentContainer => ScreenScrollContainer.ContentContainer;

        private SpriteTextPlus InteractionState { get; set; }
        private SpriteTextPlus ChurnState { get; set; }
        private Button FocusedButton { get; set; }

        public TestButtonsGalleryScreenView(Screen screen) : base(screen)
        {
            ScreenScrollContainer = new ScrollContainer(
                new ScalableVector2(Container.Width, Container.Height),
                new ScalableVector2(Container.Width, Math.Max(ContentHeight, Container.Height)))
            {
                Parent = Container,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                Tint = Color.Transparent
            };
            ScreenScrollContainer.Scrollbar.Tint = ScrollbarColor;
            ScreenScrollContainer.Scrollbar.Width = 6;

            CreateHeader();

            var typesPanel = CreatePanel(-310, "BUTTON TYPES + SIZING");
            CreateButtonTypes(typesPanel);

            var statesPanel = CreatePanel(310, "CONTENT + BEHAVIOR");
            CreateBehaviorExamples(statesPanel);
        }

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "BUTTONS GALLERY", 26)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 25,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "Hover, hold, click-to-focus, drag, clip, and churn every button implementation.", 17)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 61,
                Tint = MutedColor
            };
        }

        private Container CreatePanel(float x, string title)
        {
            var panel = new Container
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = 100,
                Size = new ScalableVector2(570, 630)
            };

            new Sprite
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Size = panel.Size,
                Image = WobbleAssets.WhiteBox,
                Tint = PanelColor
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), title, 18)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 20,
                Y = 16,
                Tint = Color.White
            };

            return panel;
        }

        private void CreateButtonTypes(Container panel)
        {
            CreateSectionLabel(panel, "FIXED SIZE", 54);
            var rounded = new RoundedButton
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(20, 82),
                Size = new ScalableVector2(245, 44),
                Tint = AccentColor
            };
            rounded.SetLabel(FontManager.GetWobbleFont("inter-bold"), "RoundedButton", 16, Color.White);
            TrackInteraction(rounded, "RoundedButton");

            var text = new TextButton(WobbleAssets.WhiteBox, "inter-bold", "TextButton", 16)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(285, 82),
                Size = new ScalableVector2(245, 44),
                Tint = PurpleColor
            };
            TrackInteraction(text, "TextButton");

            var image = CreateImageButton(panel, "ImageButton", 20, 145, 245, AccentColor);
            TrackInteraction(image, "ImageButton");

            var draggable = new DraggableButton(WobbleAssets.WhiteBox)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(285, 145),
                Size = new ScalableVector2(245, 44),
                Tint = PurpleColor
            };
            AddCenteredLabel(draggable, "DraggableButton — drag me");
            TrackInteraction(draggable, "DraggableButton");

            CreateSectionLabel(panel, "AUTOMATIC / CONTENT SIZE", 215);
            var automaticRounded = new RoundedButton
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(20, 244),
                WidthMode = ButtonSizeMode.Auto,
                HeightMode = ButtonSizeMode.Fixed,
                Height = 44,
                AutoSizePadding = new Vector2(36, 0),
                Tint = AccentColor
            };
            automaticRounded.SetLabel(FontManager.GetWobbleFont("inter-semibold"), "Auto Rounded", 16, Color.White);
            TrackInteraction(automaticRounded, "Auto RoundedButton");

            var automaticText = new TextButton(WobbleAssets.WhiteBox, "inter-bold", "Natural TextButton", 16)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(285, 244),
                Tint = PurpleColor
            };
            automaticText.Size = new ScalableVector2(automaticText.Width + 32, 44);
            TrackInteraction(automaticText, "Natural TextButton");

            CreateSectionLabel(panel, "INTERACTION STATE", 320);
            InteractionState = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"),
                "Hover or press a button", 17)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 20,
                Y = 351,
                Tint = FocusColor
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "Clicked button keeps a gold focus border. Held state is reported live.", 15)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 20,
                Y = 382,
                Tint = MutedColor
            };

            CreateTextOverflowExamples(panel);
        }

        private void CreateTextOverflowExamples(Container panel)
        {
            CreateSectionLabel(panel, "ROUNDED BUTTON TEXT OVERFLOW", 430);

            var alwaysMarquee = CreateMarqueeButton(panel, 458, AccentColor,
                "Always running marquee — this label is intentionally much wider than its rounded button");
            alwaysMarquee.IsActive = true;
            alwaysMarquee.StartDelayMilliseconds = 0;
            TrackInteraction((Button) alwaysMarquee.Parent, "Always-on marquee");

            var hoverMarquee = CreateMarqueeButton(panel, 513, PurpleColor,
                "Hover marquee — this long label only starts moving while the button is hovered");
            hoverMarquee.StartDelayMilliseconds = 0;
            var hoverButton = (Button) hoverMarquee.Parent;
            hoverButton.Hovered += (sender, args) => hoverMarquee.IsActive = true;
            hoverButton.LeftHover += (sender, args) => hoverMarquee.IsActive = false;
            TrackInteraction(hoverButton, "Hover marquee");

            var clippedText = CreateMarqueeButton(panel, 568, DisabledColor,
                "Clipped only — this text remains stationary and must never escape the rounded button bounds");
            clippedText.IsActive = false;
            TrackInteraction((Button) clippedText.Parent, "Clipped long label");
        }

        private MarqueeText CreateMarqueeButton(Container panel, float y, Color color, string text)
        {
            const float buttonWidth = 510;
            const float horizontalPadding = 20;

            var button = new RoundedButton
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(20, y),
                Size = new ScalableVector2(buttonWidth, 44),
                Tint = color
            };

            return new MarqueeText(FontManager.GetWobbleFont("inter-semibold"), text, 15,
                buttonWidth - horizontalPadding * 2)
            {
                Parent = button,
                Alignment = Alignment.MidCenter
            };
        }

        private void CreateBehaviorExamples(Container panel)
        {
            CreateSectionLabel(panel, "LONG, EMOJI + LOCALIZED LABELS", 54);
            var longLabel = new RoundedButton
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(20, 82),
                Size = new ScalableVector2(510, 44),
                Tint = PurpleColor
            };
            longLabel.SetLabel(FontManager.GetWobbleFont("inter-semibold"),
                "Save changes • Запази промените • 保存 🎉", 16, Color.White);
            TrackInteraction(longLabel, "Localized label");

            CreateSectionLabel(panel, "DISABLED", 145);
            var disabled = CreateImageButton(panel, "Disabled — clicks are ignored", 20, 173, 510, DisabledColor);
            disabled.IsClickable = false;
            disabled.Alpha = 0.55f;

            CreateSectionLabel(panel, "NESTED + CLIPPED", 235);
            var clip = new HorizontalClippingContainer
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(20, 263),
                Size = new ScalableVector2(510, 78)
            };

            var outer = CreateImageButton(clip, "Parent button", 0, 0, 510, AccentColor);
            outer.Height = 78;
            outer.UsePreviousSpriteBatchOptions = true;
            TrackInteraction(outer, "Parent button");

            var nested = new RoundedButton
            {
                Parent = clip,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(430, 18),
                Size = new ScalableVector2(210, 42),
                Tint = PurpleColor,
                Depth = -1,
                UsePreviousSpriteBatchOptions = true
            };
            nested.SetLabel(FontManager.GetWobbleFont("inter-bold"), "Nested + clipped", 15, Color.White);
            TrackInteraction(nested, "Nested clipped button");

            CreateSectionLabel(panel, "RAPID CREATE / DESTROY", 367);
            var churn = new RoundedButton
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(20, 395),
                Size = new ScalableVector2(245, 44),
                Tint = AccentColor
            };
            churn.SetLabel(FontManager.GetWobbleFont("inter-bold"), "Churn 250 buttons", 16, Color.White);
            churn.Clicked += (sender, args) => RunChurn(panel);

            ChurnState = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), "Not run", 16)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 285,
                Y = 407,
                Tint = MutedColor
            };
        }

        private void RunChurn(Container parent)
        {
            for (var i = 0; i < 250; i++)
            {
                var button = new RoundedButton
                {
                    Parent = parent,
                    Size = new ScalableVector2(80 + i % 7, 30 + i % 5),
                    Tint = i % 2 == 0 ? AccentColor : PurpleColor
                };
                button.SetLabel(FontManager.GetWobbleFont("inter-regular"), i.ToString(), 12, Color.White);
                button.Destroy();
            }

            ChurnState.Text = "Created + destroyed 250 ✓";
            ChurnState.Tint = new Color(105, 230, 166);
        }

        private ImageButton CreateImageButton(Drawable parent, string label, float x, float y, float width, Color color)
        {
            var button = new ImageButton(WobbleAssets.WhiteBox)
            {
                Parent = parent,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(width, 44),
                Tint = color
            };
            AddCenteredLabel(button, label);
            return button;
        }

        private void AddCenteredLabel(Drawable button, string label) => new SpriteTextPlus(
            FontManager.GetWobbleFont("inter-bold"), label, 15)
        {
            Parent = button,
            Alignment = Alignment.MidCenter,
            Tint = Color.White,
            UsePreviousSpriteBatchOptions = true
        };

        private void TrackInteraction(Button button, string name)
        {
            button.AddBorder(Color.Transparent, 3);
            button.Hovered += (sender, args) => InteractionState.Text = name + ": HOVERED";
            button.LeftHover += (sender, args) =>
            {
                if (FocusedButton != button)
                    InteractionState.Text = FocusedButton == null ? "No focused button" : "FOCUSED: " + FocusedButton.GetType().Name;
            };
            button.Clicked += (sender, args) =>
            {
                if (FocusedButton?.Border != null)
                    FocusedButton.Border.Tint = Color.Transparent;

                FocusedButton = button;
                button.Border.Tint = FocusColor;
                InteractionState.Text = name + ": FOCUSED (clicked)";
            };
        }

        private void CreateSectionLabel(Drawable parent, string text, float y) => new SpriteTextPlus(
            FontManager.GetWobbleFont("inter-bold"), text, 14)
        {
            Parent = parent,
            Alignment = Alignment.TopLeft,
            X = 20,
            Y = y,
            Tint = MutedColor
        };

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);

            if (FocusedButton?.IsHeld == true)
                InteractionState.Text = FocusedButton.GetType().Name + ": PRESSED / HELD";
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
