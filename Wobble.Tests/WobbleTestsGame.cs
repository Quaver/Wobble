using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
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
            WindowManager.ChangeScreenResolution(new Point(1366, 768));

            // Get device period and buffer from the environment variables.
            // Must be done before calling base.Initialize().
            var env = Environment.GetEnvironmentVariables();

            if (env.Contains("WOBBLE_TESTS_BASS_DEV_PERIOD") &&
                int.TryParse(env["WOBBLE_TESTS_BASS_DEV_PERIOD"] as string, out var period))
                DevicePeriod = period;

            if (env.Contains("WOBBLE_TESTS_BASS_DEV_BUFFER") &&
                int.TryParse(env["WOBBLE_TESTS_BASS_DEV_BUFFER"] as string, out var buffer))
                DeviceBufferLength = buffer;

            base.Initialize();

            Window.AllowUserResizing = true;

            Content.RootDirectory = "Content";

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

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-bold"))
                BitmapFontFactory.AddFont("exo2-bold", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-bold.ttf"));

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-regular"))
                BitmapFontFactory.AddFont("exo2-regular", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-regular.ttf"));

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-semibold"))
                BitmapFontFactory.AddFont("exo2-semibold", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-semibold.ttf"));

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-medium"))
                BitmapFontFactory.AddFont("exo2-medium", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-medium.ttf"));

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
        }
    }
}
