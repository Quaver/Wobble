using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.DrawableScaling
{
    public class TestDrawableScalingScreenView : ScreenView
    {
        /// <summary>
        ///     Green box sprite.
        /// </summary>
        public Sprite GreenBox { get; }

        public Sprite TopLeft { get; }

        public Sprite Mid { get; }

        public Sprite BottomRight { get; }

        public SpriteText SpriteText { get; }

        /// <summary>
        ///     The background color for the scene.
        /// </summary>
        private Color BackgroundColor { get; set; } = Color.CornflowerBlue;

        private Vector2 _scale = Vector2.One;
        private bool _rotating;

        private SpriteText DebugText { get; }

        public Vector2 Scale
        {
            get => _scale;
            private set
            {
                _scale = value;
                GreenBox.Scale = value;
                Mid.Scale = value;
                BottomRight.Scale = value;
                DebugText.ScheduleUpdate(() => DebugText.Text = $"Scale: {value}");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestDrawableScalingScreenView(Screen screen) : base(screen)
        {
            #region GREEN_BOX

            GreenBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(200, 400),
                Tint = Color.Green,
                Position = new ScalableVector2(0, 0),
                Alignment = Alignment.MidCenter,
                Pivot = Vector2.One
            };

            TopLeft = new Sprite()
            {
                Parent = GreenBox,
                Size = new ScalableVector2(100, 200),
                Tint = Color.Blue,
                Position = new ScalableVector2(0, 0),
                Alignment = Alignment.TopLeft,
                // Pivot = new Vector2(1, 1)
            };

            BottomRight = new Sprite()
            {
                Parent = GreenBox,
                Size = new ScalableVector2(100, 200),
                Tint = Color.YellowGreen,
                Position = new ScalableVector2(0, 0),
                Alignment = Alignment.BotRight,
                Pivot = new Vector2(1, 1)
            };

            Mid = new Sprite()
            {
                Parent = GreenBox,
                Size = new ScalableVector2(50, 50),
                Tint = Color.Red,
                Position = new ScalableVector2(0, 0),
                Alignment = Alignment.MidCenter,
            };

            SpriteText = new SpriteText("exo2-bold", "AAA", 20)
            {
                Parent = BottomRight,
                Size = new ScalableVector2(50, 50),
                Alignment = Alignment.MidCenter
            };

            GreenBox.AddBorder(Color.White, 2);

            TopLeft.AddBorder(Color.Red, 2);

            Mid.AddBorder(Color.LightYellow, 2);

            BottomRight.AddBorder(Color.Purple, 2);

            #endregion

            DebugText = new SpriteText("exo2-bold", "Hello, World!", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopRight
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);

            if (KeyboardManager.IsUniqueKeyPress(Keys.OemCloseBrackets))
                Scale *= 2;

            if (KeyboardManager.IsUniqueKeyPress(Keys.OemOpenBrackets))
                Scale /= 2;

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
                _rotating = !_rotating;

            if (_rotating)
                GreenBox.Rotation += 0.0001f;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}