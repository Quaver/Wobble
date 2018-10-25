using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.BitmapFonts;
using Wobble.Input;
using Wobble.IO;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Tests.Screens.Selection;
using Wobble.Window;

namespace Wobble.Tests
{
    public class WobbleTestsGame : WobbleGame
    {
        protected override bool IsReadyToUpdate { get; set; }

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

            Resources.AddStore(new DllResourceStore("Wobble.Tests.Resources.dll"));
            Textures.Load();

            // (This should normally be done at the start of the game.)
            // Load up a font to use by a byte[] / ResourceStore.
            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-bold"))
                BitmapFontFactory.AddFont("exo2-bold", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-bold.otf"));

            // (This should normally be done at the start of the game.)
            // Load up a font to use by a byte[] / ResourceStore.
            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-regular"))
                BitmapFontFactory.AddFont("exo2-regular", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-regular.otf"));

            IsReadyToUpdate = true;

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
            if (!IsReadyToUpdate)
                return;

            base.Update(gameTime);

            // TODO: Your global update logic goes here.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ScreenManager.ChangeScreen(new SelectionScreen());
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

            base.Draw(gameTime);

            // TODO: Your global draw logic goes here.
        }
    }
}
