using System;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.GlobalUiScale
{
    public class TestGlobalUiScaleScreenView : ScreenView
    {
        private BindableInt ScalePercent { get; }

        private SpriteTextPlus ScaleText { get; }

        private SpriteTextPlus ClickText { get; }

        private Slider ScaleSlider { get; set; }

        private int AppliedScalePercent { get; set; } = 100;

        private bool WasAdjustingScale { get; set; }

        private int ClickCount { get; set; }

        public TestGlobalUiScaleScreenView(Screen screen) : base(screen)
        {
            WindowManager.UiScale = 1.0f;

            ScalePercent = new BindableInt(100, (int)(WindowManager.MinUiScale * 100), (int)(WindowManager.MaxUiScale * 100));

            new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = new Color(24, 28, 34),
                UseGlobalUiScale = false
            };

            ScaleText = new SpriteTextPlus(FontManager.GetWobbleFont("exo2-semibold"), "Global UI Scale: applied 100%", 24)
            {
                Parent = Container,
                Position = new ScalableVector2(24, 18),
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("exo2-regular"),
                "Drag the slider and release to apply. The screen stays fixed while UI elements scale.",
                16)
            {
                Parent = Container,
                Position = new ScalableVector2(24, 54),
                Tint = new Color(210, 216, 224),
                MaxWidth = 700
            };

            ScaleSlider = new Slider(ScalePercent, new Vector2(360, 12), WobbleAssets.WhiteBox)
            {
                Parent = Container,
                Position = new ScalableVector2(28, 110),
                Tint = new Color(72, 92, 126)
            };
            ScaleSlider.ActiveColor.Tint = new Color(62, 180, 137);
            ScaleSlider.ProgressBall.Size = new ScalableVector2(22, 22);
            ScaleSlider.ProgressBall.Tint = new Color(245, 248, 252);
            ScalePercent.ValueChanged += OnScalePercentChanged;

            var resetButton = new TextButton(WobbleAssets.WhiteBox, "exo2-semibold", "Reset 100%", 16)
            {
                Parent = Container,
                Position = new ScalableVector2(420, 90),
                Size = new ScalableVector2(150, 50),
                Tint = new Color(64, 132, 194),
                Text = { Tint = Color.White }
            };
            resetButton.Clicked += (sender, args) => ScalePercent.Value = 100;

            var clickButton = new TextButton(WobbleAssets.WhiteBox, "exo2-semibold", "Click target", 16)
            {
                Parent = Container,
                Position = new ScalableVector2(590, 90),
                Size = new ScalableVector2(170, 50),
                Tint = new Color(192, 84, 96),
                Text = { Tint = Color.White }
            };
            clickButton.Clicked += (sender, args) =>
            {
                ClickCount++;
                ClickText.Text = $"Clicks: {ClickCount}";
            };

            ClickText = new SpriteTextPlus(FontManager.GetWobbleFont("exo2-regular"), "Clicks: 0", 18, false)
            {
                Parent = Container,
                Position = new ScalableVector2(590, 150),
                Tint = Color.White
            };

            CreateAlignmentMarkers();
            CreateAssetSamples();
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);

            if (ScaleSlider.MouseInHoldSequence)
            {
                WasAdjustingScale = true;
            }
            else if (WasAdjustingScale)
            {
                WasAdjustingScale = false;
                ApplyScalePercent();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(12, 14, 18));
            Container?.Draw(gameTime);

            GameBase.Game.SpriteBatch.End();
        }

        public override void Destroy()
        {
            WindowManager.UiScale = 1.0f;
            ScalePercent.ValueChanged -= OnScalePercentChanged;
            Container?.Destroy();
        }

        private void OnScalePercentChanged(object sender, BindableValueChangedEventArgs<int> args)
        {
            ScaleText.Text = ScaleSlider.MouseInHoldSequence
                ? $"Global UI Scale: pending {args.Value}% (applied {AppliedScalePercent}%)"
                : $"Global UI Scale: applied {args.Value}%";

            if (!ScaleSlider.MouseInHoldSequence)
                ApplyScalePercent();
        }

        private void ApplyScalePercent()
        {
            AppliedScalePercent = ScalePercent.Value;
            WindowManager.UiScale = AppliedScalePercent / 100f;
            ScaleText.Text = $"Global UI Scale: applied {AppliedScalePercent}%";
        }

        private void CreateAlignmentMarkers()
        {
            CreateMarker("Top Left", Alignment.TopLeft, new ScalableVector2(24, 190), new Color(245, 204, 92));
            CreateMarker("Top Right", Alignment.TopRight, new ScalableVector2(-24, 190), new Color(108, 194, 255));
            CreateMarker("Center", Alignment.MidCenter, new ScalableVector2(0, 0), new Color(132, 224, 132));
            CreateMarker("Bottom Right", Alignment.BotRight, new ScalableVector2(-24, -24), new Color(255, 134, 164));
        }

        private void CreateMarker(string label, Alignment alignment, ScalableVector2 position, Color tint)
        {
            var marker = new Sprite
            {
                Parent = Container,
                Alignment = alignment,
                Position = position,
                Size = new ScalableVector2(150, 70),
                Tint = tint
            };

            new Sprite
            {
                Parent = marker,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(132, 52),
                Tint = new Color(24, 28, 34)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("exo2-semibold"), label, 15)
            {
                Parent = marker,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };
        }

        private void CreateAssetSamples()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont("exo2-semibold"), "Asset samples", 20)
            {
                Parent = Container,
                Position = new ScalableVector2(24, 300),
                Tint = Color.White
            };

            new Sprite
            {
                Parent = Container,
                Position = new ScalableVector2(28, 344),
                Size = new ScalableVector2(72, 72),
                Image = Textures.LeftButtonSquare,
                Tint = Color.White
            };

            new Sprite
            {
                Parent = Container,
                Position = new ScalableVector2(118, 344),
                Size = new ScalableVector2(72, 72),
                Image = Textures.RightButtonSquare,
                Tint = Color.White
            };

            var atlas = Textures.TestSpriteSheet;
            for (var i = 0; i < Math.Min(6, atlas.Count); i++)
            {
                new Sprite
                {
                    Parent = Container,
                    Position = new ScalableVector2(220 + i * 54, 354),
                    Size = new ScalableVector2(42, 42),
                    Image = atlas[i],
                    Tint = Color.White
                };
            }

            new SpriteTextPlus(FontManager.GetWobbleFont("exo2-regular"),
                "Cached SpriteTextPlus should resize its render target as the global scale changes.",
                16)
            {
                Parent = Container,
                Position = new ScalableVector2(24, 440),
                Tint = new Color(210, 216, 224),
                MaxWidth = 600
            };
        }
    }
}
