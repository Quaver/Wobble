using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Imgui
{
    public class TestImGuiScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private TestImGuiMenu TestImGuiMenu { get; }

        private List<ImageButton> Boxes { get; } = new List<ImageButton>();

        /// <summary>
        /// </summary>
        private ImageButton Box { get; }

        /// <summary>
        /// </summary>
        private Random RNG { get; } = new Random();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestImGuiScreenView(Screen screen) : base(screen)
        {
            TestImGuiMenu = new TestImGuiMenu();

            // Make a button
            // ReSharper disable once ObjectCreationAsStatement
            for (var i = 0; i < 5000; i++)
            {
                Boxes.Add(new ImageButton(WobbleAssets.WhiteBox, (sender, args) => Logger.Important("CLICKED", LogType.Runtime, false))
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    Size = new ScalableVector2(100, 100),
                    Tint = Color.Crimson,
                });
            }

            // ReSharper disable once ObjectCreationAsStatement
            var box2 = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(75, 75),
                Position = new ScalableVector2(0, 400),
                Tint = Color.Red,
                Rotation = 60
            };

            box2.MoveToX(1200, Easing.OutQuint, 4000);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TestImGuiMenu.Update(gameTime);

            if (TestImGuiMenu.Rotation)
            {
                for (var i = 0; i < Boxes.Count; i++)
                {
                    var box = Boxes[i];

                    if (box.Animations.Count == 0)
                    {
                        if (box.Y > 0)
                            box.MoveToY(-300, Easing.Linear, 3000);
                        else
                            box.MoveToY(300, Easing.Linear, 3000);
                    }
                }
            }

            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            var color = Color.CornflowerBlue;

           /* if (TestImGuiMenu.Lightshow)
            {
                color = new Color(RNG.Next(255), RNG.Next(255), RNG.Next(255));
                Box.Tint = new Color(RNG.Next(255), RNG.Next(255), RNG.Next(255));
            }
            else
            {
                Box.Tint = Color.Crimson;
            }*/

            GameBase.Game.GraphicsDevice.Clear(color);
            Container?.Draw(gameTime);

            GameBase.Game.SpriteBatch.End();
            TestImGuiMenu.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
            TestImGuiMenu.Destroy();
        }
    }
}