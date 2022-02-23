using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Animations;
using Wobble.Screens;

namespace GreenBox.Screens
{
    public class GreenBoxScreenView : ScreenView
    {
        /// <summary>
        ///     The GreenBox sprite that's drawn to the screen
        /// </summary>
        private Sprite GreenBox { get; }

        /// <summary>
        ///     Keeps track of if the green box has moved back to its original position.
        /// </summary>
        private bool GreenBoxMovedBack { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Initialize your ScreenView and all Drawables here.
        /// </summary>
        /// <param name="screen"></param>
        public GreenBoxScreenView(Screen screen) : base(screen)
        {
            GreenBox = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Image = WobbleAssets.WhiteBox, // Wobble Comes with included assets. Here we use a white box
                Tint = Color.Green, // Color the box green.
                Size = new ScalableVector2(75, 75),
                Animations =
                {
                    // Add an animation to move and animate the object.
                    // In this case we're changing the X property to 500 in 2 seconds (2000 milliseconds).
                    // Animations use easing functions, you can learn more about this
                    // at the following resource: https://easings.net/
                    new Animaion(AnimationProperty.X, Easing.InElastic, 0, 500, 2000) 
                }
            };

            Console.WriteLine($@"Done Loading GreenBoxScreenView!");
        }

        /// <inheritdoc />
        /// <summary>
        ///     All of the update logic for the ScreenView.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // When transformations are comlpete, they are automatically removed
            // from the Sprite's list. Here we'll want to add another one to move the
            // GreenBox back to its original position!
            if (GreenBox.Animations.Count == 0 && !GreenBoxMovedBack)
            {
                // Add the new animation
                GreenBox.Animations.Add(new Animation(AnimationProperty.X, Easing.EaseInBounce, GreenBox.X, 0, 1000));

                // Set GreenBoxMovedBack to true so it doesn't keep adding transformations once it's complete.
                GreenBoxMovedBack = true;
            }

            // Update the container and all of its containing Drawables.
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Access the instance of the game off of GameBase and clear the graphics device (like normal)
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the entire container.
            Container?.Draw(gameTime);

            // Attempt to end the SpriteBatch. An exception can be thrown if no sprites
            // are in the container.
            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Destroy the container when the screen is destroyed.
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}
