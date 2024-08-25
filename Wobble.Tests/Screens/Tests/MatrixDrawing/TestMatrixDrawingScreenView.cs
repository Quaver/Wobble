using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.MatrixDrawing
{
    public class TestMatrixDrawingScreenView : ScreenView
    {
        private Texture2D texture;
        private Matrix2 quarterBottomRightTransform;
        private Matrix transform3d;
        private float rotation;
        private SpriteText spriteText;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestMatrixDrawingScreenView(Screen screen) : base(screen)
        {

            texture = WobbleAssets.Wallpaper;
            spriteText = new SpriteText("exo2-regular", "", 15) { Parent = Container };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
            var axis = Vector3.Up + Vector3.Left;
            axis.Normalize();
            var size = 200;
            transform3d = Matrix.Identity 
                // * Matrix.CreateTranslation(-250, 0, 0)
                * Matrix.CreateScale(size, size, 1)
                          * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(axis, rotation))
                * Matrix.CreateTranslation(250, 250, size * 1.5f)
                // * Matrix.CreateTranslation(0, 0, -2)
                          // * Matrix.CreateTranslation(500, 500, 0)
                ;
            quarterBottomRightTransform = Matrix2.CreateScale(WindowManager.VirtualScreen / 2) *
                                          new Matrix2(1, 0, 0.1f, 1, 0, 0) *
                                          Matrix2.CreateTranslation(WindowManager.VirtualScreen / 2);
            Container?.Update(gameTime);
            spriteText.ScheduleUpdate(() => spriteText.Text = $"Rot: {rotation:0.0000}");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);

            GameBase.Game.SpriteBatch.Draw(texture, ref quarterBottomRightTransform, null, layerDepth: rotation * 10);
            GameBase.Game.SpriteBatch.Draw(texture, ref transform3d, null);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}