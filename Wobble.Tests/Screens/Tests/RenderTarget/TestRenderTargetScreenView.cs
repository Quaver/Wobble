using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.RenderTarget
{
    public class TestRenderTargetScreenView : ScreenView
    {
        private RenderTargetContainer Target { get; set; }
        private RenderTargetContainer NestedTarget { get; set; }
        private FilledRectangleSprite MovingChild { get; set; }
        private SpriteTextPlus Status { get; }
        private bool Compact { get; set; }
        private bool NestedVisible { get; set; } = true;
        private bool Faded { get; set; }
        private bool Scaled { get; set; }

        public TestRenderTargetScreenView(Screen screen) : base(screen)
        {
            CreateTarget();

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "RENDER TARGET CONTAINER", 26)
            {
                Parent = Container, Alignment = Alignment.TopCenter, Y = 24, Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "R resize/clip  •  N nested  •  O opacity  •  S scale  •  D recreate", 17)
            {
                Parent = Container, Alignment = Alignment.TopCenter, Y = 61, Tint = Color.LightGray
            };

            Status = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), "", 16)
            {
                Parent = Container, Alignment = Alignment.BotCenter, Y = -24, Tint = Color.White
            };
        }

        private void CreateTarget()
        {
            Target?.Destroy();
            Target = new RenderTargetContainer(new ScalableVector2(Container.Width, Container.Height))
            {
                Parent = Container,
                Pivot = Vector2.Zero
            };

            new FilledRectangleSprite
            {
                Parent = Target,
                Position = new ScalableVector2(90, 115),
                Size = new ScalableVector2(520, 285),
                Tint = new Color(35, 69, 104),
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            MovingChild = new FilledRectangleSprite
            {
                Parent = Target,
                Position = new ScalableVector2(120, 205),
                Size = new ScalableVector2(105, 70),
                Tint = Color.Orange,
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            NestedTarget = new RenderTargetContainer(new ScalableVector2(Container.Width, Container.Height))
            {
                Parent = Target,
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            new FilledRectangleSprite
            {
                Parent = NestedTarget,
                Position = new ScalableVector2(690, 145),
                Size = new ScalableVector2(330, 230),
                Tint = new Color(104, 55, 135),
                Pivot = Vector2.Zero,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-bold"), "NESTED TARGET", 21)
            {
                Parent = NestedTarget,
                Position = new ScalableVector2(755, 240),
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            ApplyModes();
        }

        private void ApplyModes()
        {
            var targetWidth = Compact ? Math.Max(1, Container.Width - 420) : Container.Width;
            var targetHeight = Compact ? Math.Max(1, Container.Height - 220) : Container.Height;
            if (Target.Width != targetWidth || Target.Height != targetHeight)
                Target.Size = new ScalableVector2(targetWidth, targetHeight);

            Target.Alpha = Faded ? 0.45f : 1f;
            var targetScale = Scaled ? new Vector2(0.82f, 1.12f) : Vector2.One;
            if (Target.Scale != targetScale)
                Target.Scale = targetScale;

            if (NestedTarget.Width != Container.Width || NestedTarget.Height != Container.Height)
                NestedTarget.Size = new ScalableVector2(Container.Width, Container.Height);

            NestedTarget.Visible = NestedVisible;
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            MovingChild.X = 120 + (float)((Math.Sin(gameTime.TotalGameTime.TotalSeconds * 1.7) + 1) * 180);

            if (KeyboardManager.IsUniqueKeyPress(Keys.R)) Compact = !Compact;
            if (KeyboardManager.IsUniqueKeyPress(Keys.N)) NestedVisible = !NestedVisible;
            if (KeyboardManager.IsUniqueKeyPress(Keys.O)) Faded = !Faded;
            if (KeyboardManager.IsUniqueKeyPress(Keys.S)) Scaled = !Scaled;
            if (KeyboardManager.IsUniqueKeyPress(Keys.D))
            {
                Target.RecreateRenderTarget();
                NestedTarget.RecreateRenderTarget();
            }

            ApplyModes();
            Status.Text = $"Target {Target.Width:0}×{Target.Height:0}  generation {Target.RenderTargetGeneration}  |  " +
                          $"nested generation {NestedTarget.RenderTargetGeneration}  |  moving child must not increase either count";
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(16, 22, 31));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}
