using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.Rotation
{
    public class TestRotationScreenView : ScreenView
    {
        /// <summary>
        ///     Green box sprite.
        /// </summary>
        public Sprite GreenBox { get; }

        public Sprite BlueBox { get; }

        public Sprite Shaft { get; }

        /// <summary>
        ///     The background color for the scene.
        /// </summary>
        private Color BackgroundColor { get; set; } = Color.CornflowerBlue;

        private bool _rotating = true;

        private SpriteText DebugText { get; }

        private float _increment = 0.0005f;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestRotationScreenView(Screen screen) : base(screen)
        {
            #region GREEN_BOX

            GreenBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(50, 100),
                Tint = Color.Green,
                Position = new ScalableVector2(0, 0),
                Alignment = Alignment.MidCenter,
                // Pivot = Vector2.Zero
            };

            BlueBox = new Sprite()
            {
                Parent = GreenBox,
                Size = new ScalableVector2(100, 25),
                Tint = Color.Blue,
                Position = new ScalableVector2(50, 100),
                Alignment = Alignment.TopLeft,
                Pivot = new Vector2(1, 1)
            };

            Shaft = new Sprite()
            {
                Parent = GreenBox,
                Size = new ScalableVector2(100 / MathF.Cos(25f / 100), 5),
                Tint = Color.Blue,
                Position = new ScalableVector2(50, 100),
                Alignment = Alignment.TopLeft,
                Rotation = MathF.Atan(25f / 100),
                Pivot = Vector2.Zero
            };

            GreenBox.AddBorder(Color.White, 2);

            BlueBox.AddBorder(Color.Red, 2);

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

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
                _rotating = !_rotating;

            if (KeyboardManager.IsUniqueKeyPress(Keys.OemCloseBrackets))
                _increment *= 2;

            if (KeyboardManager.IsUniqueKeyPress(Keys.OemOpenBrackets))
                _increment /= 2;

            if (_rotating)
            {
                GreenBox.Rotation += _increment;
                BlueBox.Rotation += _increment;
            }

            DebugText.ScheduleUpdate(() => DebugText.Text = $"{GreenBox.Rotation:0.00} {_increment}/tick");
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