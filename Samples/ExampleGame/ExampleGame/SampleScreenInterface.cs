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
    public class SampleScreenInterface : ScreenInterface
    {
        public Sprite Person;

        public SampleScreenInterface(SampleScreen screen) : base(screen)
        {
            var game = (ExampleGame) GameBase.Game;

            Person = new Sprite
            {
                Image = game.Spongebob,
                Size = new ScalableVector2(400, 400),
                Alignment = Alignment.MidCenter,
                Tint = Color.Blue,
                Parent = Container
            };
        }

        public override void Update(GameTime gameTime)
        {
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

            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, WindowManager.Scale);
            Container.Draw(gameTime);
            GameBase.Game.SpriteBatch.End();
        }

        public override void Destroy() => Container.Destroy();
    }
}