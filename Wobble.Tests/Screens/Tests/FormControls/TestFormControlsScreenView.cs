using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.FormControls
{
    public class TestFormControlsScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(17, 24, 32);
        private static readonly Color PanelColor = new Color(31, 41, 51);
        private static readonly Color AccentColor = new Color(15, 186, 229);
        private static readonly Color SelectedColor = new Color(117, 92, 222);
        private static readonly Color MutedColor = new Color(126, 139, 151);
        private static readonly Color SuccessColor = new Color(105, 230, 166);

        private readonly Bindable<bool> controlsEnabled = new Bindable<bool>(true);
        private readonly BindableInt sliderValue = new BindableInt(65, 0, 100);
        private readonly BindableDouble progressValue = new BindableDouble(65, 0, 100);
        private readonly List<Textbox> tabTextboxes = new List<Textbox>();

        private Checkbox enabledCheckbox;
        private Slider slider;
        private HorizontalSelector selector;
        private SpriteTextPlus enabledState;
        private SpriteTextPlus sliderState;
        private SpriteTextPlus selectorState;
        private SpriteTextPlus textboxState;

        public TestFormControlsScreenView(Screen screen) : base(screen)
        {
            CreateHeader();
            CreateInteractivePanel();
            CreateStatePanel();

            controlsEnabled.ValueChanged += (sender, args) => ApplyEnabledState(args.Value);
            sliderValue.ValueChanged += (sender, args) => progressValue.Value = args.Value;

            ApplyEnabledState(controlsEnabled.Value);
        }

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "FORM CONTROLS GALLERY", 26)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 30,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "Interact with the controls to inspect hovered, focused, selected, and disabled states.", 17)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 67,
                Tint = MutedColor
            };
        }

        private void CreateInteractivePanel()
        {
            var panel = CreatePanel(-285, 115, 520, 535, "INTERACTIVE CONTROLS");

            CreateLabel(panel, "Checkbox — enables the form", 62);
            enabledCheckbox = new Checkbox(controlsEnabled, new Vector2(28, 28), WobbleAssets.WhiteBox,
                WobbleAssets.WhiteBox, false)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = 92,
                Tint = SuccessColor
            };
            enabledCheckbox.Hovered += (sender, args) => enabledState.Text = "HOVERED";
            enabledCheckbox.LeftHover += (sender, args) => UpdateEnabledStateText();

            enabledState = CreateValue(panel, "ENABLED", 70, 96, SuccessColor);

            CreateLabel(panel, "Slider + ProgressBar", 145);
            slider = new Slider(sliderValue, new Vector2(335, 8), WobbleAssets.WhiteBox)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = 184,
                Tint = MutedColor
            };
            slider.ActiveColor.Tint = AccentColor;
            slider.ProgressBall.Tint = Color.White;
            slider.Hovered += (sender, args) => sliderState.Text = $"HOVERED  ·  {sliderValue.Value}%";
            slider.LeftHover += (sender, args) => UpdateSliderStateText();
            sliderValue.ValueChanged += (sender, args) => UpdateSliderStateText();

            new ProgressBar(new Vector2(335, 10), progressValue, PanelColor, AccentColor, false)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = 210
            };
            sliderState = CreateCenteredValue(panel, "65%", 228, AccentColor);
            sliderState.X = -64.5f;

            CreateLabel(panel, "HorizontalSelector — selected state", 266);
            selector = new HorizontalSelector(new List<string> { "Default", "Hovered", "Selected" },
                new ScalableVector2(260, 38), FontManager.GetWobbleFont("inter-semibold"), 17,
                Textures.LeftButtonSquare, Textures.RightButtonSquare, new ScalableVector2(38, 38), 8,
                (value, index) => selectorState.Text = $"SELECTED: {value.ToUpperInvariant()}", 2, true)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 73,
                Y = 302,
                Tint = PanelColor
            };
            selector.SelectedItemText.Tint = Color.White;
            StyleSelectorButtons(selector, SelectedColor, Color.White);
            selectorState = CreateCenteredValue(panel, "SELECTED: SELECTED", 350, SelectedColor);

            CreateLabel(panel, "Textbox + TextboxTabControl", 390);
            tabTextboxes.Add(CreateTextbox(panel, 28, 424, 142, "First name"));
            tabTextboxes.Add(CreateTextbox(panel, 180, 424, 142, "Last name"));
            tabTextboxes.Add(CreateTextbox(panel, 332, 424, 142, "Tag"));
            tabTextboxes[0].Focused = true;
            tabTextboxes[0].Cursor.Visible = true;

            new TextboxTabControl(tabTextboxes)
            {
                Parent = panel
            };

            textboxState = CreateCenteredValue(panel, "FOCUSED: FIRST NAME  ·  TAB / SHIFT+TAB TO MOVE", 478,
                SuccessColor);
        }

        private void CreateStatePanel()
        {
            var panel = CreatePanel(285, 115, 520, 535, "SUPPORTED STATES");

            CreateLabel(panel, "Disabled Checkbox", 62);
            new Checkbox(new Bindable<bool>(false), new Vector2(28, 28), WobbleAssets.WhiteBox,
                WobbleAssets.WhiteBox, true)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = 92,
                Tint = MutedColor,
                Alpha = 0.4f,
                IsClickable = false
            };
            CreateValue(panel, "DISABLED", 70, 96, MutedColor);

            CreateLabel(panel, "Disabled Slider", 145);
            var disabledSlider = new Slider(new BindableInt(35, 0, 100), new Vector2(335, 8), WobbleAssets.WhiteBox)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = 184,
                Tint = MutedColor,
                Alpha = 0.4f,
                IsClickable = false
            };
            disabledSlider.ActiveColor.Tint = MutedColor;
            disabledSlider.ProgressBall.Tint = MutedColor;
            CreateValue(panel, "DISABLED", 385, 177, MutedColor);

            CreateLabel(panel, "Disabled HorizontalSelector", 230);
            var disabledSelector = new HorizontalSelector(new List<string> { "Unavailable" },
                new ScalableVector2(260, 38), FontManager.GetWobbleFont("inter-semibold"), 17,
                Textures.LeftButtonSquare, Textures.RightButtonSquare, new ScalableVector2(38, 38), 8,
                (value, index) => { }, 0, true)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 73,
                Y = 270,
                Alpha = 0.4f
            };
            disabledSelector.SelectedItemText.Tint = MutedColor;
            disabledSelector.RoundedButtonSelectLeft.IsClickable = false;
            disabledSelector.RoundedButtonSelectRight.IsClickable = false;
            StyleSelectorButtons(disabledSelector, MutedColor, BackgroundColor);

            CreateLabel(panel, "Disabled Textbox", 330);
            var disabledTextbox = CreateTextbox(panel, 28, 365, 446, "Input unavailable");
            disabledTextbox.Alpha = 0.4f;
            disabledTextbox.Button.IsClickable = false;

            CreateLabel(panel, "State guide", 430);
            CreateValue(panel, "HOVER: move pointer over an enabled control", 28, 464, AccentColor);
            CreateValue(panel, "FOCUS: click a textbox or use Tab", 28, 489, SuccessColor);
            CreateValue(panel, "SELECT: use the selector arrows", 28, 514, SelectedColor);
        }

        private Container CreatePanel(float x, float y, float width, float height, string title)
        {
            var panel = new Container
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = y,
                Size = new ScalableVector2(width, height)
            };

            new Sprite
            {
                Parent = panel,
                Size = panel.Size,
                Tint = PanelColor,
                Alpha = 0.96f
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), title, 18)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = 24,
                Tint = Color.White
            };

            return panel;
        }

        private static void CreateLabel(Container parent, string text, float y) =>
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), text, 16)
            {
                Parent = parent,
                Alignment = Alignment.TopLeft,
                X = 28,
                Y = y,
                Tint = Color.White
            };

        private static SpriteTextPlus CreateValue(Container parent, string text, float x, float y, Color color) =>
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-medium"), text, 14)
            {
                Parent = parent,
                Alignment = Alignment.TopLeft,
                X = x,
                Y = y,
                Tint = color
            };

        private static SpriteTextPlus CreateCenteredValue(Container parent, string text, float y, Color color) =>
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-medium"), text, 14)
            {
                Parent = parent,
                Alignment = Alignment.TopCenter,
                Y = y,
                Tint = color
            };

        private static Textbox CreateTextbox(Container parent, float x, float y, float width, string placeholder) =>
            new Textbox(new ScalableVector2(width, 38), FontManager.GetWobbleFont("inter-medium"), 15, "", placeholder)
            {
                Parent = parent,
                Alignment = Alignment.TopLeft,
                X = x,
                Y = y,
                Tint = BackgroundColor,
                Alpha = 0.95f,
                Focused = false
            };

        private static void StyleSelectorButtons(HorizontalSelector horizontalSelector, Color buttonColor,
            Color labelColor)
        {
            var left = horizontalSelector.RoundedButtonSelectLeft;
            var right = horizontalSelector.RoundedButtonSelectRight;

            left.CornerRadius = 8;
            right.CornerRadius = 8;
            left.Tint = buttonColor;
            right.Tint = buttonColor;

            left.Label.Text = "<";
            right.Label.Text = ">";
            left.Label.Tint = labelColor;
            right.Label.Tint = labelColor;

            // Inter's angle brackets sit slightly above their visual center at this size.
            left.Label.X = 1;
            right.Label.X = 1;
            left.Label.Y = 1;
            right.Label.Y = 1;
        }

        private void ApplyEnabledState(bool enabled)
        {
            slider.IsClickable = enabled;
            slider.Alpha = enabled ? 1 : 0.4f;
            selector.RoundedButtonSelectLeft.IsClickable = enabled;
            selector.RoundedButtonSelectRight.IsClickable = enabled;
            selector.Alpha = enabled ? 1 : 0.4f;

            foreach (var textbox in tabTextboxes)
            {
                textbox.Button.IsClickable = enabled;
                textbox.Alpha = enabled ? 0.95f : 0.4f;

                if (!enabled)
                    textbox.Focused = false;
            }

            if (enabled && !tabTextboxes.Exists(x => x.Focused))
            {
                tabTextboxes[0].Focused = true;
                tabTextboxes[0].Cursor.Visible = true;
            }

            enabledCheckbox.Tint = enabled ? SuccessColor : MutedColor;
            UpdateEnabledStateText();
        }

        private void UpdateEnabledStateText() =>
            enabledState.Text = controlsEnabled.Value ? "ENABLED" : "DISABLED";

        private void UpdateSliderStateText() => sliderState.Text = $"{sliderValue.Value}%";

        public override void Update(GameTime gameTime)
        {
            var focusedIndex = tabTextboxes.FindIndex(x => x.Focused);
            textboxState.Text = focusedIndex >= 0
                ? $"FOCUSED: {tabTextboxes[focusedIndex].PlaceholderText.ToUpperInvariant()}  ·  TAB / SHIFT+TAB TO MOVE"
                : "FOCUSED: NONE";

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
