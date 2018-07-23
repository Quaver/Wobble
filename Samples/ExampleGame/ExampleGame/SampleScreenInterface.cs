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
        /// <summary>
        ///     Test sprite
        /// </summary>
        public Sprite PersonWithShader { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public SampleScreenInterface(SampleScreen screen) : base(screen)
        {
            // Grab the game instance.
            var game = (ExampleGame) GameBase.Game;

            // Create new sprite to be drawn.
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
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            Container.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);

            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, WindowManager.Scale);
            Container.Draw(gameTime);
            GameBase.Game.SpriteBatch.End();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container.Destroy();

        /// <summary>
        ///     In this example, it when the user presses down a key, it'll change the shader's parameters.
        /// </summary>
        private void HandleInput()
        {
            // Make shader transparency rect smaller.
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.Left))
            {
                // Grab the current shader parameter.
                var currentRect = (Vector2) PersonWithShader.Shader.Parameters["p_rectangle"];

                // Grab the new width and set it.
                var newWidth = MathHelper.Clamp(currentRect.X - 20, 0, PersonWithShader.Width);

                // Change the parameters in the dictionary.
                PersonWithShader.Shader.Parameters["p_rectangle"] = new Vector2(newWidth, PersonWithShader.Height);

                // Be sure to re-set parameters after changing.
                PersonWithShader.Shader.SetParameters();
            }

            // Make shader transparency rect larger.
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.Right))
            {
                // Grab the current shader parameter.
                var currentRect = (Vector2) PersonWithShader.Shader.Parameters["p_rectangle"];

                // Grab the new width and set it.
                var newWidth = MathHelper.Clamp(currentRect.X + 20, 0, PersonWithShader.Width);

                // Change the parameters in the dictionary.
                PersonWithShader.Shader.Parameters["p_rectangle"] = new Vector2(newWidth, PersonWithShader.Height);

                // Be sure to re-set parameters after changing.
                PersonWithShader.Shader.SetParameters();
            }
        }
    }
}