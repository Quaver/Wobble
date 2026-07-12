using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.ScreenLifecycle
{
    public sealed class TestScreenLifecycleScreenView : ScreenView
    {
        internal const string PersistentKey = "screen-lifecycle-persistent";

        private static readonly Color Background = new Color(14, 18, 25);
        private static readonly Color Panel = new Color(29, 38, 50);
        private static readonly Color Accent = new Color(35, 174, 214);
        private static readonly Color Muted = new Color(171, 184, 196);

        private readonly TestScreenLifecycleScreen lifecycleScreen;
        private readonly SpriteTextPlus status;
        private readonly Sprite animatedBox;
        private double automaticSwitchAt;
        private bool automatic;

        public TestScreenLifecycleScreenView(TestScreenLifecycleScreen screen) : base(screen)
        {
            lifecycleScreen = screen;

            TooltipManager.Theme = new TooltipTheme
            {
                Fonts = new Dictionary<int, WobbleFontStore>
                {
                    {FontWeight.Regular, FontManager.GetWobbleFont("inter-regular")},
                    {FontWeight.Medium, FontManager.GetWobbleFont("inter-medium")},
                    {FontWeight.SemiBold, FontManager.GetWobbleFont("inter-semibold")},
                    {FontWeight.Bold, FontManager.GetWobbleFont("inter-bold")}
                }
            };

            // Deliberately force layout twice while the view tree is being constructed.
            Container.Size = new ScalableVector2(Math.Max(1, WindowManager.Width - 1),
                Math.Max(1, WindowManager.Height - 1));
            Container.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "SCREEN LIFECYCLE", 28)
            {
                Parent = Container, Alignment = Alignment.TopCenter, Y = 42, Tint = Color.White
            };

            status = new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"), string.Empty, 17)
            {
                Parent = Container, Alignment = Alignment.TopCenter, Y = 90, Tint = Muted
            };

            animatedBox = new Sprite
            {
                Parent = Container, Alignment = Alignment.MidCenter, X = -260, Y = 25,
                Size = new ScalableVector2(70, 70), Image = WobbleAssets.WhiteBox, Tint = Accent
            };
            animatedBox.MoveToX(260, Easing.OutQuint, 5000);
            animatedBox.AddScheduledUpdate(() => TestScreenLifecycleSession.ScheduledWork++);

            var removalHost = new Container
            {
                Parent = Container, Alignment = Alignment.MidCenter, Y = 115,
                Size = new ScalableVector2(400, 30)
            };
            new RemoveDuringUpdateDrawable
            {
                Parent = removalHost, Size = new ScalableVector2(10, 10)
            };

            CreateButton("SWITCH ONCE", -330, () => TestScreenLifecycleSession.SwitchFrom(lifecycleScreen));
            CreateButton("AUTO STRESS", -110, () =>
            {
                automatic = true;
                TestScreenLifecycleSession.Automatic = true;
                TestScreenLifecycleSession.RemainingAutomaticSwitches = 12;
                automaticSwitchAt = 0;
                DialogManager.Show(new TransitionDialog());
            });
            CreateButton("SHOW DIALOG", 110, () => DialogManager.Show(new TransitionDialog()));
            CreateButton("BACK TO TESTS", 330, () => TestScreenLifecycleSession.Exit(lifecycleScreen));
        }

        public void Activate()
        {
            if (!ScreenManager.TryGetElement<SpriteTextPlus>(PersistentKey, out var persistent))
            {
                persistent = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"),
                    "PERSISTENT ELEMENT - hover during transitions", 15)
                {
                    Parent = Container, Alignment = Alignment.BotCenter, Y = -35, Tint = Color.Gold
                };
                persistent.AddTooltip(new TooltipOptions("This tooltip target is retained across both screens.")
                {
                    HoverDelayMilliseconds = 0
                });
                ScreenManager.RegisterElement(PersistentKey, persistent);
            }

            automatic = TestScreenLifecycleSession.Automatic;
            automaticSwitchAt = 0;
            RefreshStatus();
        }

        public override void Update(GameTime gameTime)
        {
            Container.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Update(gameTime);
            DialogManager.Update(gameTime);

            if (automatic && gameTime.TotalGameTime.TotalMilliseconds >= automaticSwitchAt)
            {
                automaticSwitchAt = gameTime.TotalGameTime.TotalMilliseconds + 450;
                TestScreenLifecycleSession.RemainingAutomaticSwitches--;
                TestScreenLifecycleSession.Automatic =
                    TestScreenLifecycleSession.RemainingAutomaticSwitches > 0;
                TestScreenLifecycleSession.SwitchFrom(lifecycleScreen);
            }

            RefreshStatus();
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Background);
            Container.Draw(gameTime);
            DialogManager.Draw(gameTime);
        }

        public override void Destroy()
        {
            // Exercise a synchronous layout cascade immediately before teardown.
            Container.Size = new ScalableVector2(Math.Max(1, WindowManager.Width - 2),
                Math.Max(1, WindowManager.Height - 2));
            Container.Destroy();
        }

        private void RefreshStatus()
        {
            status.Text = $"Current: {(lifecycleScreen.IsRetained ? "RETAINED INSTANCE" : "DISPOSABLE INSTANCE")}  |  " +
                          $"AutomaticallyDestroy: {lifecycleScreen.AutomaticallyDestroyOnScreenSwitch}\n" +
                          $"Constructed {TestScreenLifecycleSession.Constructed}  Activated {TestScreenLifecycleSession.Activated}  " +
                          $"Destroyed {TestScreenLifecycleSession.Destroyed}  Scheduled {TestScreenLifecycleSession.ScheduledWork}  " +
                          $"Child removals {TestScreenLifecycleSession.RemovedChildren}\n" +
                          "Auto alternates retained/disposable screens every 450ms while animation, dialog, tooltip, resize, and removal paths stay active.";
        }

        private void CreateButton(string text, float x, Action action)
        {
            var button = new RoundedButton
            {
                Parent = Container, Alignment = Alignment.MidCenter, X = x, Y = 205,
                Size = new ScalableVector2(190, 44), Tint = Panel, CornerRadius = 4
            };
            button.SetLabel(FontManager.GetWobbleFont("inter-semibold"), text, 14, Color.White);
            button.Clicked += (sender, args) => action();
        }

        private sealed class RemoveDuringUpdateDrawable : Drawable
        {
            private bool removed;

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                if (removed)
                    return;

                removed = true;
                TestScreenLifecycleSession.RemovedChildren++;
                Parent = null;
            }

            public override void DrawToSpriteBatch()
            {
            }
        }

        private sealed class TransitionDialog : DialogScreen
        {
            public TransitionDialog() : base(0.45f)
            {
                Clicked += (sender, args) => DialogManager.Dismiss(this);
                new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"),
                    "TRANSITION DIALOG\nClick the dimmed area to dismiss", 20)
                {
                    Parent = Container, Alignment = Alignment.MidCenter, Tint = Color.White
                };
            }

            public override void CreateContent()
            {
            }

            public override void HandleInput(GameTime gameTime)
            {
            }
        }
    }
}
