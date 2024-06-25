using System;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.RenderTarget
{
    public class TestRenderTargetScreenView : ScreenView
    {
        public Container RenderTargetContainer { get; }

        public Sprite MainComponentSprite { get; }
        public Sprite CustomProjectionSprite { get; }

        public SpriteText RotationText { get; }

        public SpriteText ScaleText { get; }

        private TimeSpan _textUpdateTimer = TimeSpan.Zero;

        public float Rotation
        {
            get => MainComponentSprite.Rotation;
            set => MainComponentSprite.Rotation = value;
        }

        public Vector2 Scale
        {
            get => MainComponentSprite.Scale;
            set => MainComponentSprite.Scale = value;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestRenderTargetScreenView(Screen screen) : base(screen)
        {
            RenderTargetContainer = new Container()
            {
                Parent = Container,
                Size = new ScalableVector2(250, 500),
                Alignment = Alignment.MidCenter
            };
            new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(250, 500),
                Alignment = Alignment.MidCenter,
                Tint = new Color(0, 255, 0, 50)
            };

            MainComponentSprite = new Sprite()
            {
                Parent = RenderTargetContainer,
                Alignment = Alignment.MidCenter,
                Tint = Color.Red,
                Size = new ScalableVector2(250, 500),
                Pivot = new Vector2(0.5f, 0.5f)
            };

            CustomProjectionSprite = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(125, 250),
                Position = new ScalableVector2(-100, 0)
            };

            CustomProjectionSprite.BindProjectionContainer(RenderTargetContainer);

            RotationText = new SpriteText("exo2-bold", $"Rotation: 0", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 15,
            };
            ScaleText = new SpriteText("exo2-bold", $"Scale: 0", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 50,
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Rotation = ((float)gameTime.TotalGameTime.TotalSeconds * 0.5f) % (2 * MathF.PI);
            Scale = Vector2.One * MathF.Pow(MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds), 2);

            Container?.Update(gameTime);
            UpdateText(gameTime);
        }

        private void UpdateText(GameTime gameTime)
        {
            _textUpdateTimer += gameTime.ElapsedGameTime;
            
            if (_textUpdateTimer >= TimeSpan.FromMilliseconds(16))
            {
                RotationText.ScheduleUpdate(() => RotationText.Text = $"Rotation: {Rotation:0.000}");
                ScaleText.ScheduleUpdate(() => ScaleText.Text = $"Scale: {Scale}");
                _textUpdateTimer = TimeSpan.Zero;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}