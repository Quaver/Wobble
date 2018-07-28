using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Tests.Screens.Selection;
using Wobble.Window;

namespace Wobble.Tests
{
    public class WobbleTestsGame : WobbleGame
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;

            Graphics.PreferredBackBufferWidth = (int) WindowManager.VirtualScreen.X;
            Graphics.PreferredBackBufferHeight = (int)WindowManager.VirtualScreen.Y;

            Graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Graphics.ApplyChanges();

            GlobalUserInterface.Cursor.Hide(0);
            IsMouseVisible = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // TODO: Your asset loading + first screen goes here.
            Textures.Load();
            Fonts.Load();

            // Once the assets load, we'll start the main screen
            ScreenManager.ChangeScreen(new SelectionScreen());
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // TODO: Your disposing logic goes here.
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: Your global update logic goes here.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ScreenManager.ChangeScreen(new SelectionScreen());
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // TODO: Your global draw logic goes here.
        }
    }
}
