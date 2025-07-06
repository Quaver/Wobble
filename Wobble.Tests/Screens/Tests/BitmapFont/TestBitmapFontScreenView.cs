using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Screens;
using Color = Microsoft.Xna.Framework.Color;

namespace Wobble.Tests.Screens.Tests.BitmapFont
{
    public class TestBitmapFontScreenView : ScreenView
    {
        private MonoGame.Extended.BitmapFonts.BitmapFont font;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestBitmapFontScreenView(Screen screen) : base(screen)
        {
            font = GameBase.Game.Content.Load<MonoGame.Extended.BitmapFonts.BitmapFont>("exo2-regular");

            new SpriteTextBitmap(font, "The quick brown fox jumps over the lazy dog")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                FontSize = 72,
                MaxWidth = 300,
                Tint = Color.White,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied,
                    SamplerState = SamplerState.LinearClamp,
                }
            };

            new SpriteText("exo2-regular", "The quick brown fox jumps over the lazy dog", 12)
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}