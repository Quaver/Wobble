using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.IO;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Screens.Selection;
using Wobble.Window;
using Point = Microsoft.Xna.Framework.Point;

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

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-bold"))
                BitmapFontFactory.AddFont("exo2-bold", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-bold.ttf"));

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-regular"))
                BitmapFontFactory.AddFont("exo2-regular", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-regular.ttf"));

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-semibold"))
                BitmapFontFactory.AddFont("exo2-semibold", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-semibold.ttf"));

            if (!BitmapFontFactory.CustomFonts.ContainsKey("exo2-medium"))
                BitmapFontFactory.AddFont("exo2-medium", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-medium.ttf"));

            var font = new WobbleFontStore(20, GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-semibold.ttf"), new Dictionary<string, byte[]>()
                {
                    {"Emoji", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/symbola-emoji.ttf")},
                    {"Japanese", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/droid-sans-japanese.ttf")}
                });

            FontManager.CacheWobbleFont("exo2-semibold", font);

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
