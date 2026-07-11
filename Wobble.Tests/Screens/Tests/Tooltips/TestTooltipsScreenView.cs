using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Tooltips
{
    public class TestTooltipsScreenView : ScreenView
    {
        private readonly FilledRectangleSprite _movingTarget;

        public TestTooltipsScreenView(Screen screen) : base(screen)
        {
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

            var positions = new[]
            {
                new Vector2(15, 15), new Vector2(633, 15), new Vector2(1251, 15),
                new Vector2(1251, 344), new Vector2(1251, 673), new Vector2(633, 673),
                new Vector2(15, 673), new Vector2(15, 344)
            };
            var weights = new[] {100, 300, 400, 500, 600, 700, 800, 900};

            for (var i = 0; i < positions.Length; i++)
            {
                var target = new FilledRectangleSprite
                {
                    Parent = Container,
                    Position = new ScalableVector2(positions[i].X, positions[i].Y),
                    Size = new ScalableVector2(100, 80),
                    Tint = new Color(55 + i * 18, 130, 205 - i * 12),
                    Pivot = Vector2.Zero
                };
                target.AddTooltip(new TooltipOptions(
                    i == 2
                        ? "Long tooltip text that wraps across multiple lines and automatically chooses a position where it fits. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                        : $"{(TooltipAnchor) i}\nRequested weight: {weights[i]}")
                {
                    Anchor = (TooltipAnchor) i,
                    MaximumWidth = i == 2 ? 260 : 230,
                    Style = new TooltipStyle
                    {
                        TextWeight = weights[i],
                        BackgroundColor = i == 4 ? new Color(65, 20, 75) : (Color?) null,
                        TextColor = i == 4 ? Color.Yellow : (Color?) null,
                        TextSize = i == 5 ? 24 : (int?) null,
                        BorderColor = i == 6 ? Color.Orange : (Color?) null,
                        BorderThickness = i == 7 ? 0 : (float?) null,
                        RoundedCorners = i == 7 ? false : (bool?) null
                    }
                });
            }

            var text = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), "Text target", 24)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -80,
                Tint = Color.White
            };
            text.AddTooltip(new TooltipOptions("Tooltips attach to text too.")
            {
                Anchor = TooltipAnchor.BottomCenter,
                HoverDelayMilliseconds = 0
            });

            var button = new RoundedButton
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Position = new ScalableVector2(-70, 10),
                Size = new ScalableVector2(140, 50),
                Tint = Color.CornflowerBlue
            };
            button.SetLabel(FontManager.GetWobbleFont("inter-medium"), "Button", 18, Color.White);
            button.AddTooltip("This tooltip is attached to a button.");

            _movingTarget = new FilledRectangleSprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Position = new ScalableVector2(-30, 100),
                Size = new ScalableVector2(60, 60),
                Tint = Color.LimeGreen,
                Pivot = Vector2.Zero
            };
            _movingTarget.AddTooltip(new TooltipOptions("Moving icon target\nThe tooltip follows every frame.")
            {
                Anchor = TooltipAnchor.CenterRight,
                MaximumWidth = 200
            });
        }

        public override void Update(GameTime gameTime)
        {
            _movingTarget.X = -30 + (float) Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 180;
            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(20, 23, 28));
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();
    }
}
