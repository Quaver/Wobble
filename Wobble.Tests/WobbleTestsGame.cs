using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Diagnostics;
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

        protected override bool DrawGlobalUserInterface => true;

        private FpsCounter FpsCounter { get; set; }
        
        private SpriteTextPlus WaylandState { get; set; }

        private bool _logGc;
        private double _gcLogTimer;
        private readonly int[] _lastGcCounts = new int[3];

        private int? _testTargetFps;
        private int? _testTargetUps;
        private double _nextTestDrawTimestamp;
        private double _nextTestUpdateTimestamp;
        private TimeSpan _pendingTestUpdateElapsed;

        internal int? TestTargetFps => _testTargetFps;

        internal int? TestTargetUps => _testTargetUps;

#if DEBUG
        private PerformanceSweep _performanceSweep;
#endif

        public WobbleTestsGame() : base(true)
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

            LocalizationManager.Configure(new ResourceManager("Wobble.Tests.Localization.Strings", typeof(WobbleTestsGame).Assembly),
                CultureInfo.GetCultureInfo("en"), embeddedResourceAssembly: typeof(WobbleTestsGame).Assembly);

            Resources.AddStore(new DllResourceStore("Wobble.Tests.Resources.dll"));

            var interFont = GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/Inter/Inter.ttf");
            var emojiFont = GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/NotoColorEmoji/NotoColorEmoji.ttf");

            CacheInterFont("inter-regular", FontWeight.Regular, interFont, emojiFont);
            CacheInterFont("inter-medium", FontWeight.Medium, interFont, emojiFont);
            CacheInterFont("inter-semibold", FontWeight.SemiBold, interFont, emojiFont);
            CacheInterFont("inter-bold", FontWeight.Bold, interFont, emojiFont);

            IsReadyToUpdate = true;

            FpsCounter = new FpsCounter(FontManager.GetWobbleFont("inter-semibold"), 18)
            {
                Parent = GlobalUserInterface,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(70, 30),
            };

            WaylandState = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), $"Wayland: {WaylandVsync}", 18)
            {
                Parent = GlobalUserInterface,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(0, -30),
                Visible = OperatingSystem.IsLinux()
            };

            // Once the assets load, we'll start the main screen
            ScreenManager.ChangeScreen(new SelectionScreen());

#if DEBUG
            if (string.Equals(Environment.GetEnvironmentVariable("WOBBLE_TESTS_PROFILE_ALL"), "1",
                    StringComparison.Ordinal))
                _performanceSweep = new PerformanceSweep(this);
#endif
        }

        private static void CacheInterFont(string name, int weight, byte[] interFont, byte[] emojiFont)
        {
            FontManager.CacheWobbleFont(name, new WobbleFontStore(20,
                new WobbleFontFace(interFont, weight: weight),
                new Dictionary<string, WobbleFontFace>()
                {
                    {"Emoji", new WobbleFontFace(emojiFont)}
                }));
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

            if (!TryCreateTestUpdateTime(gameTime, out var updateTime))
                return;

            base.Update(updateTime);

#if DEBUG
            _performanceSweep?.Update(updateTime);
#endif

            // TODO: Your global update logic goes here.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ScreenManager.ChangeScreen(new SelectionScreen());

            if (KeyboardManager.IsUniqueKeyPress(Keys.W) && OperatingSystem.IsLinux())
            {
                WaylandVsync = !WaylandVsync;
                WaylandState.Text = $"Wayland: {WaylandVsync}";
            }

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
                _gcLogTimer += updateTime.ElapsedGameTime.TotalMilliseconds;
                if (_gcLogTimer >= 1000)
                {
                    _gcLogTimer = 0;
                    LogGc("GC tick");
                }
            }
        }

        protected override bool BeginDraw()
        {
            if (!ShouldRunTestDraw())
                return false;

            return base.BeginDraw();
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Applies independent update and draw limits for the animation timing test screen.
        ///     A null value leaves that side of the loop unlimited.
        /// </summary>
        internal void SetTestFrameRates(int? targetFps, int? targetUps)
        {
            ValidateTestFrameRate(targetFps, nameof(targetFps));
            ValidateTestFrameRate(targetUps, nameof(targetUps));

            _testTargetFps = targetFps;
            _testTargetUps = targetUps;
            _pendingTestUpdateElapsed = TimeSpan.Zero;

            var now = Stopwatch.GetTimestamp();
            _nextTestDrawTimestamp = targetFps.HasValue
                ? now + Stopwatch.Frequency / (double) targetFps.Value
                : 0;
            _nextTestUpdateTimestamp = targetUps.HasValue
                ? now + Stopwatch.Frequency / (double) targetUps.Value
                : 0;
        }

        internal void ResetTestFrameRates() => SetTestFrameRates(null, null);

        private bool TryCreateTestUpdateTime(GameTime gameTime, out GameTime updateTime)
        {
            if (!_testTargetUps.HasValue)
            {
                _pendingTestUpdateElapsed = TimeSpan.Zero;
                updateTime = gameTime;
                return true;
            }

            _pendingTestUpdateElapsed += gameTime.ElapsedGameTime;

            var now = Stopwatch.GetTimestamp();
            if (now < _nextTestUpdateTimestamp)
            {
                updateTime = null;
                return false;
            }

            AdvanceTestDeadline(ref _nextTestUpdateTimestamp, _testTargetUps.Value, now);
            updateTime = new GameTime(gameTime.TotalGameTime, _pendingTestUpdateElapsed,
                gameTime.IsRunningSlowly);
            _pendingTestUpdateElapsed = TimeSpan.Zero;
            return true;
        }

        private bool ShouldRunTestDraw()
        {
            if (!_testTargetFps.HasValue)
                return true;

            var now = Stopwatch.GetTimestamp();
            if (now < _nextTestDrawTimestamp)
                return false;

            AdvanceTestDeadline(ref _nextTestDrawTimestamp, _testTargetFps.Value, now);
            return true;
        }

        private static void AdvanceTestDeadline(ref double deadline, int rate, long now)
        {
            var interval = Stopwatch.Frequency / (double) rate;
            var elapsedIntervals = Math.Floor((now - deadline) / interval) + 1;
            deadline += Math.Max(1, elapsedIntervals) * interval;
        }

        private static void ValidateTestFrameRate(int? rate, string parameterName)
        {
            if (rate <= 0)
                throw new ArgumentOutOfRangeException(parameterName, rate,
                    "A test frame rate must be greater than zero or null for unlimited.");
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

#if DEBUG
        private sealed class PerformanceSweep
        {
            private const double WarmupMilliseconds = 750;
            private const double SampleMilliseconds = 2000;

            private readonly WobbleTestsGame game;
            private readonly List<string> rows = new List<string>();
            private readonly List<double> frameTimes = new List<double>();
            private readonly List<double> updateTimes = new List<double>();
            private readonly List<double> drawTimes = new List<double>();
            private readonly List<double> screenDrawTimes = new List<double>();
            private readonly List<double> globalUiDrawTimes = new List<double>();
            private readonly List<int> drawableCounts = new List<int>();
            private int screenIndex = -1;
            private double elapsed;
            private long allocatedBytesAtSampleStart;
            private int gen0AtSampleStart;
            private int gen1AtSampleStart;
            private int gen2AtSampleStart;
            private long maximumManagedBytes;

            public PerformanceSweep(WobbleTestsGame game)
            {
                this.game = game;
                rows.Add("screen,frames,frame_avg_ms,frame_p95_ms,update_avg_ms,update_p95_ms,draw_avg_ms,draw_p95_ms," +
                         "screen_draw_avg_ms,global_ui_draw_avg_ms,drawables_avg,drawables_max,allocated_bytes," +
                         "managed_peak_bytes,gen0,gen1,gen2");
                Advance();
            }

            public void Update(GameTime gameTime)
            {
                elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (elapsed < WarmupMilliseconds)
                    return;

                if (allocatedBytesAtSampleStart == 0)
                {
                    allocatedBytesAtSampleStart = GC.GetTotalAllocatedBytes(false);
                    gen0AtSampleStart = GC.CollectionCount(0);
                    gen1AtSampleStart = GC.CollectionCount(1);
                    gen2AtSampleStart = GC.CollectionCount(2);
                }

                frameTimes.Add(PerformanceStats.FrameTimeMs);
                updateTimes.Add(PerformanceStats.UpdateTimeMs);
                drawTimes.Add(PerformanceStats.DrawTimeMs);
                screenDrawTimes.Add(PerformanceStats.ScreenDrawTimeMs);
                globalUiDrawTimes.Add(PerformanceStats.GlobalUiDrawTimeMs);
                drawableCounts.Add(PerformanceStats.DrawnDrawableCount);
                maximumManagedBytes = Math.Max(maximumManagedBytes, GC.GetTotalMemory(false));

                if (elapsed < WarmupMilliseconds + SampleMilliseconds)
                    return;

                Record();
                Advance();
            }

            private void Advance()
            {
                screenIndex++;
                if (screenIndex >= TestScreenRegistry.Screens.Count)
                {
                    var outputPath = Environment.GetEnvironmentVariable("WOBBLE_TESTS_PROFILE_OUTPUT");
                    if (string.IsNullOrWhiteSpace(outputPath))
                        outputPath = Path.Combine(AppContext.BaseDirectory, "wobble-screen-profile.csv");

                    File.WriteAllLines(outputPath, rows);
                    Console.WriteLine($"WOBBLE_PROFILE_COMPLETE={outputPath}");
                    game.Exit();
                    return;
                }

                elapsed = 0;
                allocatedBytesAtSampleStart = 0;
                maximumManagedBytes = 0;
                frameTimes.Clear();
                updateTimes.Clear();
                drawTimes.Clear();
                screenDrawTimes.Clear();
                globalUiDrawTimes.Clear();
                drawableCounts.Clear();

                var descriptor = TestScreenRegistry.Screens[screenIndex];
                Console.WriteLine($"WOBBLE_PROFILE_SCREEN={descriptor.LabelKey}");
                ScreenManager.ChangeScreen(descriptor.CreateScreen());
            }

            private void Record()
            {
                var descriptor = TestScreenRegistry.Screens[screenIndex];
                rows.Add(string.Join(",",
                    descriptor.LabelKey,
                    frameTimes.Count,
                    Average(frameTimes),
                    Percentile95(frameTimes),
                    Average(updateTimes),
                    Percentile95(updateTimes),
                    Average(drawTimes),
                    Percentile95(drawTimes),
                    Average(screenDrawTimes),
                    Average(globalUiDrawTimes),
                    Average(drawableCounts),
                    drawableCounts.Count == 0 ? 0 : drawableCounts.Max(),
                    GC.GetTotalAllocatedBytes(false) - allocatedBytesAtSampleStart,
                    maximumManagedBytes,
                    GC.CollectionCount(0) - gen0AtSampleStart,
                    GC.CollectionCount(1) - gen1AtSampleStart,
                    GC.CollectionCount(2) - gen2AtSampleStart));
            }

            private static string Average(IReadOnlyCollection<double> values) =>
                (values.Count == 0 ? 0 : values.Average()).ToString("0.000", CultureInfo.InvariantCulture);

            private static string Average(IReadOnlyCollection<int> values) =>
                (values.Count == 0 ? 0 : values.Average()).ToString("0.0", CultureInfo.InvariantCulture);

            private static string Percentile95(IEnumerable<double> values)
            {
                var sorted = values.OrderBy(value => value).ToArray();
                if (sorted.Length == 0)
                    return "0.000";

                var index = Math.Min(sorted.Length - 1, (int)Math.Ceiling(sorted.Length * 0.95) - 1);
                return sorted[index].ToString("0.000", CultureInfo.InvariantCulture);
            }
        }
#endif
    }
}
