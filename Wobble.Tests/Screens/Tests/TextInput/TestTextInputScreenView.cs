using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Form;
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
            var textbox = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(500, 30), Fonts.AllerRegular16, "", "Type to see a cool effect!", 0.90f, text =>
            {
                // Just create a new SpriteText and move it across the screen.
                // Not recommended in production, as you'll have a random SpriteText floating around still
                // a child to the container.
                new SpriteText(Fonts.AllerRegular16, text)
                {
                    Parent = Container,
                    X = -100,
                    Y = 100,
                    Transformations =
                    {
                        new Transformation(TransformationProperty.X, Easing.Linear, -100, WindowManager.Width + 500, 5000)
                    }
                };
            })
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 0.75f,
                Focused = false
            };

            // Simple text box that when the user stops typing, it will "check" if a username is available (Random number generator)
            // and display text on the screen.
            var usernameCheckTextbox = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(500, 30), Fonts.AllerRegular16, "",
                "Enter a username", 0.90f, null, (text) =>
            {
                Console.WriteLine($"Username typed when user stopped typing: " + text);

                // Generate a random number between 0 and 1 that will represent if the username
                // is taken or not.
                var val = RNG.Next(0, 2);
                new SpriteText(Fonts.AllerRegular16, val == 1 ? "Username Available" : "Username Taken")
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    Y = 100 + 30 + 5,
                    TextColor = val == 1 ? Color.LimeGreen : Color.Red,
                    Transformations =
                    {
                        new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 1000)
                    }
                };
            })
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 0.75f,
                Y = 100
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