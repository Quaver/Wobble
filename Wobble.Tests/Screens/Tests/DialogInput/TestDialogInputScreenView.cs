using System;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.DialogInput
{
    public class TestDialogInputScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(14, 18, 24);
        private static readonly Color ButtonColor = new Color(15, 186, 229);
        private static readonly Color DialogButtonColor = new Color(117, 92, 222);

        private SpriteTextPlus BackgroundCountText { get; }
        private int BackgroundClickCount { get; set; }

        public TestDialogInputScreenView(Screen screen) : base(screen)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "DIALOG INPUT", 30)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 70,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "Open the dialog, then try clicking or hovering the dimmed background button.", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 112,
                Tint = new Color(184, 195, 204)
            };

            BackgroundCountText = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), string.Empty, 20)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -55,
                Tint = Color.White
            };

            var backgroundButton = CreateButton(Container, "CLICK BACKGROUND", -130, 20, ButtonColor,
                (sender, args) =>
                {
                    BackgroundClickCount++;
                    RefreshBackgroundCount();
                });
            backgroundButton.AddTooltip(new TooltipOptions("Background tooltip") { HoverDelayMilliseconds = 0 });

            CreateButton(Container, "SHOW DIALOG", 130, 20, DialogButtonColor,
                (sender, args) => DialogManager.Show(new InputBlockingDialog()));

            RefreshBackgroundCount();
        }

        public override void Update(GameTime gameTime)
        {
            Container.Update(gameTime);
            DialogManager.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container.Draw(gameTime);
            DialogManager.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();

        private void RefreshBackgroundCount() =>
            BackgroundCountText.Text = $"Background clicks: {BackgroundClickCount}";

        private static ImageButton CreateButton(Drawable parent, string text, float x, float y, Color color,
            EventHandler clickAction)
        {
            var button = new ImageButton(WobbleAssets.WhiteBox, clickAction)
            {
                Parent = parent,
                Alignment = Alignment.MidCenter,
                X = x,
                Y = y,
                Size = new ScalableVector2(220, 48),
                Tint = color
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), text, 16)
            {
                Parent = button,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };

            return button;
        }

        private sealed class InputBlockingDialog : DialogScreen
        {
            private Sprite Panel { get; set; }
            private SpriteTextPlus DialogCountText { get; set; }
            private int DialogClickCount { get; set; }

            public InputBlockingDialog() : base(0.7f)
            {
                Clicked += (sender, args) => DialogManager.Dismiss(this);
                CreateContent();
            }

            public override void CreateContent()
            {
                Panel = new Sprite
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    Size = new ScalableVector2(520, 300),
                    Tint = new Color(31, 41, 51)
                };

                new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "MODAL DIALOG", 26)
                {
                    Parent = Panel,
                    Alignment = Alignment.TopCenter,
                    Y = 35,
                    Tint = Color.White
                };

                DialogCountText = new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                    "Dialog clicks: 0", 19)
                {
                    Parent = Panel,
                    Alignment = Alignment.TopCenter,
                    Y = 88,
                    Tint = Color.White
                };

                var dialogButton = CreateButton(Panel, "CLICK DIALOG", -120, 55, DialogButtonColor,
                    (sender, args) =>
                    {
                        DialogClickCount++;
                        DialogCountText.Text = $"Dialog clicks: {DialogClickCount}";
                    });
                dialogButton.AddTooltip(new TooltipOptions("Dialog tooltip") { HoverDelayMilliseconds = 0 });

                CreateButton(Panel, "DISMISS", 120, 55, ButtonColor,
                    (sender, args) => DialogManager.Dismiss(this));
            }

            protected override bool IsMouseInClickArea() =>
                Panel != null && !GraphicsHelper.RectangleContains(Panel.ScreenRectangle,
                    MouseManager.CurrentState.Position);

            public override void HandleInput(GameTime gameTime)
            {
            }
        }
    }
}
