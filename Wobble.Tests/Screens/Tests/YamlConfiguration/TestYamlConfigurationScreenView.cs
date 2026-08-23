using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.YamlConfiguration
{
    public sealed class TestYamlConfigurationScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(15, 20, 27);
        private static readonly Color PanelColor = new Color(28, 37, 47);
        private static readonly Color AccentColor = new Color(15, 186, 229);
        private static readonly Color PurpleColor = new Color(117, 92, 222);
        private static readonly Color SuccessColor = new Color(105, 230, 166);
        private static readonly Color FailureColor = new Color(255, 105, 120);
        private static readonly Color MutedColor = new Color(142, 154, 168);

        private readonly YamlConfigurationTestContext context;
        private readonly SpriteTextPlus actionText;
        private readonly SpriteTextPlus valuesText;
        private readonly SpriteTextPlus yamlText;
        private readonly SpriteTextPlus diagnosticsText;

        public TestYamlConfigurationScreenView(Screen screen) : base(screen)
        {
            context = new YamlConfigurationTestContext();

            CreateHeader();
            var actions = CreatePanel(-430, 105, 360, 545, "PLAYER ACTIONS");
            var state = CreatePanel(0, 105, 440, 545, "MAIN / PLAYER / EFFECTIVE");
            var checks = CreatePanel(430, 105, 360, 545, "DETERMINISTIC SELF-CHECKS");

            CreateActionButtons(actions);
            actionText = CreateText(actions, "", 18, 455, 14, AccentColor);
            CreateText(actions, "FILES: <build>/YamlConfigurationTest/", 18, 495, 11, MutedColor);

            valuesText = CreateText(state, "", 20, 58, 15, Color.White);
            yamlText = CreateText(state, "", 20, 330, 13, MutedColor);

            CreateCheckResults(checks);
            diagnosticsText = CreateText(checks, "", 18, 400, 12, MutedColor);

            RefreshState();
        }

        public override void Update(GameTime gameTime) => Container.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container.Draw(gameTime);
        }

        public override void Destroy()
        {
            Container.Destroy();
        }

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "YAML CONFIGURATION", 27)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 24,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "Typed snapshots • sparse player overrides • opt-in editing", 16)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 62,
                Tint = MutedColor
            };
        }

        private void CreateActionButtons(Container panel)
        {
            CreateButton(panel, "Set accent override", 58, AccentColor, context.SetAccent);
            CreateButton(panel, "Reset only accent", 108, PurpleColor, context.ResetAccent);
            CreateButton(panel, "Reset panel subtree", 158, PurpleColor, context.ResetPanel);
            CreateButton(panel, "Reset all overrides", 208, PurpleColor, context.ResetAll);
            CreateButton(panel, "Attempt non-editable SkinId", 258, FailureColor,
                context.AttemptNonEditableSet);
            CreateButton(panel, "Reload from disk", 308, AccentColor, context.Reload);
        }

        private void CreateButton(Container parent, string label, float y, Color color, Action action)
        {
            var button = new RoundedButton((sender, args) =>
            {
                action();
                RefreshState();
            })
            {
                Parent = parent,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(18, y),
                Size = new ScalableVector2(324, 38),
                CornerRadius = 5,
                Tint = color
            };
            button.SetLabel(FontManager.GetWobbleFont("inter-semibold"), label, 14, Color.White);
        }

        private void CreateCheckResults(Container panel)
        {
            var y = 54f;
            foreach (var check in context.Checks)
            {
                CreateText(panel, (check.Passed ? "PASS  " : "FAIL  ") + check.Name, 18, y, 12,
                    check.Passed ? SuccessColor : FailureColor);
                y += 16;
            }

            var passed = context.Checks.Count(x => x.Passed);
            CreateText(panel, $"{passed}/{context.Checks.Count} CHECKS PASSED", 18, 350, 14,
                passed == context.Checks.Count ? SuccessColor : FailureColor);
        }

        private void RefreshState()
        {
            var main = context.Config.GetMainSnapshot();
            var snapshot = context.Config.GetSnapshot();
            valuesText.Text =
                "MAIN DEFAULT\n" +
                $"  accent: {main.Panel.AccentColor}\n" +
                $"  Opacity: {main.Panel.Opacity:0.00}\n" +
                $"  SkinId: {main.SkinId} (LOCKED)\n\n" +
                "EFFECTIVE\n" +
                $"  accent: {snapshot.Panel.AccentColor}\n" +
                $"  Opacity: {snapshot.Panel.Opacity:0.00}\n" +
                $"  SkinId: {snapshot.SkinId}\n" +
                $"  Fonts: {string.Join(", ", snapshot.Fonts)}";

            yamlText.Text = "PLAYER YAML (ON DISK)\n" + context.ReadPlayerYaml();
            actionText.Text = context.LastAction;

            diagnosticsText.Text = context.Config.Warnings.Count == 0
                ? "WARNINGS\nnone"
                : "WARNINGS\n" + string.Join("\n", context.Config.Warnings.Take(5));
        }

        private Container CreatePanel(float x, float y, float width, float height, string title)
        {
            var panel = new Container
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(width, height)
            };

            new Sprite
            {
                Parent = panel,
                Size = panel.Size,
                Tint = PanelColor,
                Alpha = 0.98f
            };

            CreateText(panel, title, 18, 18, 15, Color.White, true);
            return panel;
        }

        private SpriteTextPlus CreateText(Container parent, string text, float x, float y, int size, Color color,
            bool bold = false) => new SpriteTextPlus(
            FontManager.GetWobbleFont(bold ? "inter-bold" : "inter-medium"), text, size)
        {
            Parent = parent,
            Alignment = Alignment.TopLeft,
            X = x,
            Y = y,
            Tint = color
        };
    }
}
