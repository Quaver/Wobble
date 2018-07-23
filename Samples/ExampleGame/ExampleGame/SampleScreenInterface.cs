using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

namespace ExampleGame
{
    public class SampleScreenInterface : ScreenInterface
    {
        public Sprite PersonWithShader;

        public SampleScreenInterface(SampleScreen screen) : base(screen)
        {
            var game = (ExampleGame) GameBase.Game;

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Image = game.Spongebob,
                Size = new ScalableVector2(100, 100),
                Alignment = Alignment.TopLeft,
                Parent = Container,
            };

            PersonWithShader = new Sprite
            {
                Image = game.Spongebob,
                Size = new ScalableVector2(400, 400),
                Alignment = Alignment.MidCenter,
                Tint = Color.Blue,
                Parent = Container,
                Shader = new Shader(ResourceStore.semi_transparent, new Dictionary<string, object>
                {
                    {"p_dimensions", new Vector2(400, 400)},
                    {"p_position", new Vector2(0, 0)},
                    {"p_rectangle", new Vector2(200, 400)},
                    {"p_alpha", 0f}
                })
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Image = game.Spongebob,
                Size = new ScalableVector2(100, 100),
                Alignment = Alignment.MidLeft,
                Parent = Container,
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                PersonWithShader.Size = new ScalableVector2(700, 700);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                PersonWithShader.Size = new ScalableVector2(400, 400);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                PersonWithShader.Alignment = Alignment.TopLeft;
                PersonWithShader.Position = new ScalableVector2(400, 0);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                PersonWithShader.Alignment = Alignment.MidRight;
                PersonWithShader.Position = new ScalableVector2(-400, 0);
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