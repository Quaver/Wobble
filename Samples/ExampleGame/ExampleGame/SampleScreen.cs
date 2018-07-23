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
        public Container Container;
        public Sprite Person;

        public SampleScreen()
        {
            var game = (ExampleGame) GameBase.Game;

            Container = new Container();

            Person = new Sprite
            {
                Image = game.Spongebob,
                Size = new ScalableVector2(400, 400),
                Alignment = Alignment.MidCenter,
                Tint = Color.Blue,
                Parent = Container
            };
        }

        public void Update(GameTime gameTime)
        {
            Container.Update(gameTime);

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
            Container.Draw(gameTime);
            game.SpriteBatch.End();
        }

        public void Destroy()
        {
        }
    }
}
