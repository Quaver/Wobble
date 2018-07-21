using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Screens;
using Wobble.Window;

namespace ExampleGame
{
    public class SampleScreen : IGameScreen
    {
        public SampleScreen()
        {
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            var game = (ExampleGame)WobbleGame.Instance;
            game.GraphicsDevice.Clear(Color.CornflowerBlue);

            game.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, WindowManager.Scale);
            game.SpriteBatch.Draw(game.Spongebob, new Rectangle(0, 0, 400, 400), Color.White);
            game.SpriteBatch.End();
        }

        public void Destroy()
        {
        }
    }
}
