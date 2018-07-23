using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

namespace ExampleGame
{
    public class SampleScreen : IGameScreen
    {
        public Sprite Person;

        public SampleScreen()
        {
            var game = (ExampleGame) GameBase.Game;

            Person = new Sprite
            {
                Image = game.Spongebob,
                Size = new ScalableVector2(400, 400),
                Alignment = Alignment.MidCenter,
                Tint = Color.Blue
            };
        }

        public void Update(GameTime gameTime)
        {
            Person.Update(gameTime);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                Person.Size = new ScalableVector2(700, 700);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                Person.Size = new ScalableVector2(400, 400);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                Person.Alignment = Alignment.TopLeft;
                Person.Position = new ScalableVector2(400, 0);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                Person.Alignment = Alignment.MidRight;
                Person.Position = new ScalableVector2(-400, 0);
            }
        }

        public void Draw(GameTime gameTime)
        {
            var game = (ExampleGame)GameBase.Game;
            game.GraphicsDevice.Clear(Color.CornflowerBlue);

            game.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, WindowManager.Scale);
            Person.Draw(gameTime);
            game.SpriteBatch.End();
        }

        public void Destroy()
        {
        }
    }
}
