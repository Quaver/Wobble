using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenBox.Screens;
using Microsoft.Xna.Framework;
using Wobble;
using Wobble.Screens;

namespace GreenBox
{
    public class MyGame : WobbleGame
    {
        /// <inheritdoc />
        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // TODO: Your initialization code goes here.

            // Hide global game cursor from showing.
            GlobalUserInterface.Cursor.Hide(0);

            // Show the real mouse cursor.
            IsMouseVisible = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // TODO: Your asset loading code goes here.

            // Change to the GreenBox screen
            ScreenManager.ChangeScreen(new GreenBoxScreen());
        }

        /// <inheritdoc />
        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // TODO: Your disposing logic goes here.
        }

        /// <inheritdoc />
        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: Your global update logic goes here. Anything updated here will be updated on every screen.
        }

        /// <inheritdoc />
        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // TODO: Your global draw logic goes here. Anything drawn here will be drawn on top of every screen.
        }
    }
}
