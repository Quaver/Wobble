using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.Sprites.Text;
using Wobble.Helpers;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Joystick
{
    public class TestJoystickScreenView : ScreenView
    {
        private List<SpriteTextPlus> State { get; } = new List<SpriteTextPlus>();

        public TestJoystickScreenView(Screen screen) : base(screen)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (State.Count != JoystickManager.CurrentState.Buttons.Length)
            {
                foreach (var text in State)
                    text.Destroy();
                State.Clear();

                var font = FontManager.GetWobbleFont("exo2-semibold");
                float y = 0;
                for (var key = 0; key < JoystickManager.CurrentState.Buttons.Length; ++key)
                {
                    var text = new SpriteTextPlus(font, $"Button {key}:") { Parent = Container, Y = y };
                    y += text.Height;
                    State.Add(text);
                }
            }

            for (var key = 0; key < JoystickManager.CurrentState.Buttons.Length; ++key)
            {
                if (JoystickManager.CurrentState.Buttons[key] == ButtonState.Pressed)
                    State[key].Text = $"Button {key}: Down";
                else
                    State[key].Text = $"Button {key}:";
            }

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}