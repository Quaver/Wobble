using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Debugging;
using Wobble.Input;
using Wobble.IO;
using Wobble.Logging;
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

        private FpsCounter FpsCounter { get; set; }
        
        private bool _logGc;
        private double _gcLogTimer;
        private readonly int[] _lastGcCounts = new int[3];

        public WobbleTestsGame()
        {
        }

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

            var fonts = new List<string> { "exo2-bold", "exo2-regular", "exo2-semibold", "exo2-medium" };
            foreach (var fontName in fonts)
            {
                FontManager.CacheWobbleFont(fontName, new WobbleFontStore(20, GameBase.Game.Resources.Get($"Wobble.Tests.Resources/Fonts/{fontName}.ttf")));
            }

            var japaneseFont = new WobbleFontStore(20, GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/exo2-semibold.ttf"), new Dictionary<string, byte[]>()
                {
                    {"Emoji", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/symbola-emoji.ttf")},
                    {"Japanese", GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/droid-sans-japanese.ttf")}
                });

            FontManager.CacheWobbleFont("exo2-semibold-japanese", japaneseFont);

            IsReadyToUpdate = true;

            FpsCounter = new FpsCounter(FontManager.GetWobbleFont("exo2-semibold"), 18)
            {
                Parent = GlobalUserInterface,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(70, 30),
            };

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

            if (KeyboardManager.IsUniqueKeyPress(Keys.F10))
            {
                _logGc = !_logGc;
                _gcLogTimer = 0;
                Logger.Debug($"GC logging {(_logGc ? "enabled" : "disabled")}.", LogType.Runtime);
                LogGc("GC toggle");
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.F9))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                LogGc("GC forced");
            }

            if (_logGc)
            {
                _gcLogTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_gcLogTimer >= 1000)
                {
                    _gcLogTimer = 0;
                    LogGc("GC tick");
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

            base.Draw(gameTime);
        }

        private void LogGc(string tag)
        {
            var totalBytes = GC.GetTotalMemory(false);
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            var delta0 = gen0 - _lastGcCounts[0];
            var delta1 = gen1 - _lastGcCounts[1];
            var delta2 = gen2 - _lastGcCounts[2];

            _lastGcCounts[0] = gen0;
            _lastGcCounts[1] = gen1;
            _lastGcCounts[2] = gen2;

            Logger.Debug(
                $"{tag}: managed={totalBytes / (1024 * 1024)}MB gen0={gen0}(+{delta0}) gen1={gen1}(+{delta1}) gen2={gen2}(+{delta2})",
                LogType.Runtime);
        }
    }
}
