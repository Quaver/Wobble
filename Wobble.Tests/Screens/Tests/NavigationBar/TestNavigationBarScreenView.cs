using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Navigation;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.NavigationBars
{
    public class TestNavigationBarScreenView : ScreenView
    {
        private static readonly Color PageBackground = new Color(17, 24, 32);
        private static readonly Color BarBackground = new Color(31, 41, 51);
        private static readonly Color Blue = new Color(15, 186, 229);
        private static readonly Color Purple = new Color(117, 92, 222);
        private static readonly Color Green = new Color(105, 230, 166);

        private NavigationBar TopBar { get; }

        private NavigationBar BottomBar { get; }

        private SpriteTextPlus ManualItem { get; }

        private SpriteTextPlus Status { get; }

        private RoundedButton RuntimeButton { get; set; }

        private bool RuntimeButtonAttached { get; set; }

        private int ManualUpdateCount { get; set; }

        public TestNavigationBarScreenView(Screen screen) : base(screen)
        {
            var bold = FontManager.GetWobbleFont("inter-bold");
            var regular = FontManager.GetWobbleFont("inter-regular");

            TopBar = new NavigationBar(WindowManager.Width, 40, BarBackground)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                EdgePadding = 18,
                ItemSpacing = 10
            };

            TopBar.AddRoundedButton(NavigationBarRegion.Left, new NavigationBarButtonOptions
            {
                Icon = WobbleAssets.WhiteBox,
                IconSize = new Vector2(12, 12),
                Text = "FIXED",
                Font = bold,
                Height = 28,
                BackgroundColor = Blue,
                ClickAction = (sender, args) => Status.Text = "Fixed left button clicked"
            });

            TopBar.AddRoundedButton(NavigationBarRegion.Center, new NavigationBarButtonOptions
            {
                Icon = WobbleAssets.WhiteBox,
                IconSize = new Vector2(14, 14),
                Text = "AUTO-SIZED CENTER",
                Font = bold,
                WidthMode = ButtonSizeMode.Auto,
                HeightMode = ButtonSizeMode.Auto,
                AutoSizePadding = new Vector2(34, 16),
                BackgroundColor = Purple,
                ClickAction = (sender, args) => Status.Text = "Auto-sized center button clicked"
            });

            ManualItem = new SpriteTextPlus(regular, "MANUAL 0", 17, false)
            {
                Tint = Green
            };
            TopBar.Add(NavigationBarRegion.Right, ManualItem);

            BottomBar = new NavigationBar(WindowManager.Width, 40)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                EdgePadding = 18,
                ItemSpacing = 10
            };

            BottomBar.AddRoundedButton(NavigationBarRegion.Left, new NavigationBarButtonOptions
            {
                Text = "TRANSPARENT FOOTER",
                Font = bold,
                WidthMode = ButtonSizeMode.Auto,
                Height = 28,
                AutoSizePadding = new Vector2(28, 12),
                BackgroundColor = new Color(58, 69, 80)
            });

            BottomBar.Add(NavigationBarRegion.Center, new PulsingSprite
            {
                Image = WobbleAssets.WhiteBox,
                Size = new ScalableVector2(28, 28),
                Tint = Green
            });

            BottomBar.AddRoundedButton(NavigationBarRegion.Right, new NavigationBarButtonOptions
            {
                Text = "RIGHT",
                Font = bold,
                Height = 28,
                BackgroundColor = Blue
            });

            new SpriteTextPlus(bold, "NAVIGATION BAR", 28)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 135,
                Tint = Color.White
            };

            new SpriteTextPlus(regular,
                "M: update custom item  |  A: add/remove button  |  R: clear/restore right region", 17)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 180,
                Tint = new Color(190, 200, 210)
            };

            Status = new SpriteTextPlus(regular, "Navbar and footer use normal Wobble update/draw behavior.", 18, false)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Green
            };
        }

        public override void Update(GameTime gameTime)
        {
            Container.Update(gameTime);

            if (KeyboardManager.IsUniqueKeyPress(Keys.M))
            {
                ManualUpdateCount++;
                ManualItem.Text = $"MANUAL {ManualUpdateCount}";
                TopBar.RefreshLayout();
                Status.Text = "Custom item updated and layout refreshed";
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                ToggleRuntimeButton(bold: FontManager.GetWobbleFont("inter-bold"));

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
                ToggleRightRegion();
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(PageBackground);
            Container.Draw(gameTime);
        }

        public override void Destroy()
        {
            Container.Destroy();

            if (RuntimeButton != null && !RuntimeButton.IsDisposed)
                RuntimeButton.Destroy();

            if (!ManualItem.IsDisposed)
                ManualItem.Destroy();
        }

        private void ToggleRuntimeButton(WobbleFontStore bold)
        {
            if (RuntimeButtonAttached)
            {
                TopBar.Remove(RuntimeButton);
                RuntimeButtonAttached = false;
                Status.Text = "Runtime button removed without being destroyed";
                return;
            }

            if (RuntimeButton == null)
            {
                RuntimeButton = TopBar.AddRoundedButton(NavigationBarRegion.Left, new NavigationBarButtonOptions
                {
                    Text = "ADDED",
                    Font = bold,
                    WidthMode = ButtonSizeMode.Auto,
                    Height = 42,
                    BackgroundColor = Green,
                    ForegroundColor = PageBackground
                });
                RuntimeButtonAttached = true;
                Status.Text = "Runtime button added";
                return;
            }

            TopBar.Add(NavigationBarRegion.Left, RuntimeButton);
            RuntimeButtonAttached = true;
            Status.Text = "Existing runtime button restored";
        }

        private void ToggleRightRegion()
        {
            if (ManualItem.Parent == TopBar)
            {
                TopBar.Clear(NavigationBarRegion.Right);
                Status.Text = "Right region cleared";
            }
            else
            {
                TopBar.Add(NavigationBarRegion.Right, ManualItem);
                Status.Text = "Right region restored";
            }
        }

        private class PulsingSprite : Sprite
        {
            private double Elapsed { get; set; }

            public override void Update(GameTime gameTime)
            {
                Elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
                Alpha = 0.55f + 0.45f * (float)Math.Sin(Elapsed / 250);
                base.Update(gameTime);
            }
        }
    }
}
