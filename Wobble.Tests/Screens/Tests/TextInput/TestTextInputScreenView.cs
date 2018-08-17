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
                Alpha = 0.75f
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