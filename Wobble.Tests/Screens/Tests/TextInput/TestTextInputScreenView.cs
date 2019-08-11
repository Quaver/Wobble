using System;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;
using Color = Microsoft.Xna.Framework.Color;

namespace Wobble.Tests.Screens.Tests.TextInput
{
    public class TestTextInputScreenView : ScreenView
    {
        public Random RNG = new Random();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestTextInputScreenView(Screen screen) : base(screen)
        {
            // Simple text box that when submitted, will send the text flying across the screen.
            var textbox = new Textbox(new ScalableVector2(500, 36), FontManager.GetWobbleFont("exo2-semibold"),
                24, "", "Type to see a cool effect!")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 0.75f,
                Focused = false
            };

            textbox.OnSubmit += text =>
            {
                // Just create a new SpriteText and move it across the screen.
                // Not recommended in production, as you'll have a random SpriteText floating around still
                // a child to the container.
                new SpriteText("exo2-bold", text, 18)
                {
                    Parent = Container,
                    X = -100,
                    Y = 100,
                    Animations =
                    {
                        new Animation(AnimationProperty.X, Easing.Linear, -100, WindowManager.Width + 500, 5000)
                    }
                };
            };

            var regexpTextbox = new Textbox(new ScalableVector2(500, 30), FontManager.GetWobbleFont("exo2-semibold"),
                14, "", "This should only allow you to type numbers")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 100,
                Tint = Color.Black,
                Alpha = 0.75f,
                Focused = false,
                AllowedCharacters = new Regex("^[0-9]*$")
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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}