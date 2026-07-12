using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Audio;
using Wobble.Discord;
using Wobble.Graphics;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Debugging;
using Wobble.Input;
using Wobble.IO;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Platform;
using Wobble.Platform.Linux;
using Wobble.Screens;
using Wobble.Window;
using NativeLibrary = Wobble.Platform.Linux.NativeLibrary;

namespace Wobble
{
    /// <summary>
    ///    The main game class. This will handle all the lower level details that should not
    ///    be worried about. It takes care of the majority of the low level and boilerplate stuff, so that
    ///    all you have to do is just write the game.
    /// </summary>
    public abstract class WobbleGame : Game
    {
        static readonly Predicate<SpriteBatch> _beginCalled;

        /// <summary>
        ///     The current working directory of the executable.
        /// </summary>
        public static string WorkingDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Device period to pass to AudioManager.Initialize().
        /// </summary>
        public int? DevicePeriod { get; set; } = null;

        /// <summary>
        ///     Device buffer length to pass to AudioManager.Initialize().
        /// </summary>
        public int? DeviceBufferLength { get; set; } = null;

        /// <summary>
        /// </summary>
        public GraphicsDeviceManager Graphics { get; }

        /// <summary>
        ///    Used for drawing textures
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }
        
        /// <summary>
        ///     Thread ID of the main UI thread.
        /// </summary>
        public int MainThreadId { get; }

        /// <summary>
        ///     The amount of time elapsed since the previous frame in Milliseconds.
        /// </summary>
        public double TimeSinceLastFrame { get; private set; }

        /// <summary>
        ///     The amount of time the game has been running, as a double so it doesn't break above 1000 FPS and maintains precision.
        /// </summary>
        private double TimeRunningPrecise { get; set; }

        /// <summary>
        ///     The amount of time the game has been running.
        /// </summary>
        public long TimeRunning => (long)TimeRunningPrecise;

        /// <summary>
        ///     For any sprites that are being displayed globally, this is the container that it should
        ///     be a child of.
        /// </summary>
        public GlobalUserInterface GlobalUserInterface { get; }

        /// <summary>
        ///     Dictates if the game is ready to set up.
        /// </summary>
        protected abstract bool IsReadyToUpdate { get; set; }

        /// <summary>
        ///     All the resources used by the game.
        /// </summary>
        public ResourceStore<byte[]> Resources { get; set; } = new ResourceStore<byte[]>();

        /// <summary>
        /// </summary>
        private object ScheduledRenderTargetDrawsLock { get; } = new object();

        /// <summary>
        /// </summary>
        private List<Action> ScheduledRenderTargetDraws { get; } = new List<Action>();

        /// <summary>
        /// </summary>
        private List<Action> ScheduledRenderTargetDrawsToRun { get; } = new List<Action>();

        /// <summary>
        ///     The sprite used for clearing the alpha channel. Its alpha must be 1 (fully opaque) and its color does not matter.
        /// </summary>
        private readonly Sprite alphaOneSprite;

#if DEBUG
        private DebugOverlay DebugOverlay { get; set; }

        private long DebugDrawStartedTimestamp { get; set; }

        private GameTime DebugDrawGameTime { get; set; }

        private double DebugScheduledRenderTargetDrawMs { get; set; }

        private double DebugScreenDrawMs { get; set; }

        private double DebugGlobalUiDrawMs { get; set; }

        private int DebugDrawnDrawableCount { get; set; }
#endif

        /// <summary>
        ///     Whether the base game owns drawing the global UI. Derived games that already draw it themselves should leave this disabled.
        /// </summary>
        protected virtual bool DrawGlobalUserInterface => false;

        /// <summary>
        ///     Initializes <see cref="_beginCalled"/>.
        /// </summary>
        static WobbleGame()
        {
            var field = typeof(SpriteBatch).GetField("_beginCalled", BindingFlags.Instance | BindingFlags.NonPublic);
            var getter = new DynamicMethod(nameof(_beginCalled), typeof(bool), new[] { typeof(SpriteBatch) }, true);
            var il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);

            _beginCalled = (Predicate<SpriteBatch>)getter.CreateDelegate(typeof(Predicate<SpriteBatch>));
        }

        /// <summary>
        ///     Schedules work that must run during the draw phase.
        /// </summary>
        /// <param name="action"></param>
        public void ScheduleRenderTargetDraw(Action action)
        {
            if (action == null)
                return;

            lock (ScheduledRenderTargetDrawsLock)
                ScheduledRenderTargetDraws.Add(action);
        }

        /// <summary>
        ///     Creates a game with embedded resources as a content manager.
        /// </summary>
        protected WobbleGame(bool preferWayland = false) : base(preferWayland)
        {
            Directory.SetCurrentDirectory(WorkingDirectory);
            Environment.CurrentDirectory = WorkingDirectory;
            NativeAssemblies.Copy();

            Graphics = new GraphicsDeviceManager(this)
            {
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            };

            GameBase.Game = this;
            GlobalUserInterface = new GlobalUserInterface();
            MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Required for libbass_fx.so to load properly on Linux and not crash (see https://github.com/ppy/osu/issues/2852).
                NativeLibrary.Load("libbass.so", NativeLibrary.LoadFlags.RTLD_LAZY | NativeLibrary.LoadFlags.RTLD_GLOBAL);
            }

            alphaOneSprite = new Sprite
            {
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    // We want to copy the source alpha and leave the destination color.
                    BlendState = new BlendState
                    {
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.Zero,
                        ColorSourceBlend = Blend.Zero,
                        ColorDestinationBlend = Blend.One
                    }
                }
            };
        }

        /// <summary>
        ///     Attempts to initialize a new sprite, if <see cref="SpriteBatch.End"/> had been called.
        /// </summary>
        /// <returns>Whether <see cref="SpriteBatch.Begin"/> had been called.</returns>
        public bool TryBeginBatch()
        {
            var ret = _beginCalled(SpriteBatch);

            if (!ret)
                SpriteBatch.Begin();

            return ret;
        }

        /// <summary>
        ///     Attempts to flush all batched draw calls, if <see cref="SpriteBatch.Begin"/> had been called.
        /// </summary>
        /// <returns>Whether <see cref="SpriteBatch.End"/> had been called.</returns>
        public bool TryEndBatch()
        {
            var ret = _beginCalled(SpriteBatch);

            if (ret)
                SpriteBatch.End();

            return ret;
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Resources.AddStore(new DllResourceStore("Wobble.Resources.dll"));

            LogManager.Initialize();
            AudioManager.Initialize(DevicePeriod, DeviceBufferLength);
            Window.ClientSizeChanged += WindowManager.OnClientSizeChanged;

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            WobbleAssets.Load();

#if DEBUG
            DebugOverlay = new DebugOverlay();
#endif
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
#if DEBUG
            DebugOverlay?.Destroy();
#endif

            WobbleAssets.Dispose();
            TextureManager.Dispose();
            Resources?.Dispose();
            Window.ClientSizeChanged -= WindowManager.OnClientSizeChanged;
            WindowManager.UnHookEvents();
            AudioManager.Dispose();
            DiscordManager.Dispose();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

#if DEBUG
            PerformanceStats.BeginUpdate(gameTime);
            var updateStartedTimestamp = Stopwatch.GetTimestamp();
            var phaseStartedTimestamp = updateStartedTimestamp;
            double inputUpdateMs;
            double screenUpdateMs;
            double globalUiUpdateMs;
            double audioUpdateMs;
            double audioLogUpdateMs;
#endif

            // Update the time since the last frame and the game's clock.
            TimeSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;
            TimeRunningPrecise += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Keep the window updated with the current resolution.
            WindowManager.Update();
            MouseManager.Update();
            KeyboardManager.Update();
            JoystickManager.Update();

#if DEBUG
            inputUpdateMs = ElapsedMilliseconds(phaseStartedTimestamp);
            phaseStartedTimestamp = Stopwatch.GetTimestamp();
#endif

            ScreenManager.Update(gameTime);

            TooltipManager.Update(gameTime);

#if DEBUG
            screenUpdateMs = ElapsedMilliseconds(phaseStartedTimestamp);
            phaseStartedTimestamp = Stopwatch.GetTimestamp();
#endif

            AudioManager.Update(gameTime);

#if DEBUG
            audioUpdateMs = ElapsedMilliseconds(phaseStartedTimestamp);
            phaseStartedTimestamp = Stopwatch.GetTimestamp();
#endif

            // Update the global sprite container
            GlobalUserInterface.Update(gameTime);

#if DEBUG
            DebugOverlay?.Update(gameTime);
            globalUiUpdateMs = ElapsedMilliseconds(phaseStartedTimestamp);
            phaseStartedTimestamp = Stopwatch.GetTimestamp();
#endif

            // Keep the RPC client up-to-date.
            DiscordManager.Client?.Invoke();
            LogManager.Update(gameTime);
            Logger.Update();

#if DEBUG
            audioLogUpdateMs = audioUpdateMs + ElapsedMilliseconds(phaseStartedTimestamp);
            PerformanceStats.RecordUpdateTimings(inputUpdateMs, screenUpdateMs, globalUiUpdateMs, audioLogUpdateMs,
                ElapsedMilliseconds(updateStartedTimestamp));
#endif

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

#if DEBUG
            DebugDrawStartedTimestamp = Stopwatch.GetTimestamp();
            DebugDrawGameTime = gameTime;
            var phaseStartedTimestamp = DebugDrawStartedTimestamp;
#endif

            ScheduledRenderTargetDrawsToRun.Clear();

            lock (ScheduledRenderTargetDrawsLock)
            {
                ScheduledRenderTargetDrawsToRun.AddRange(ScheduledRenderTargetDraws);
                ScheduledRenderTargetDraws.Clear();
            }

            var scheduledRenderTargetDrawCount = ScheduledRenderTargetDrawsToRun.Count;

#if DEBUG
            PerformanceStats.BeginDraw(scheduledRenderTargetDrawCount, ScreenManager.CurrentScreenName);
#endif

            for (var i = scheduledRenderTargetDrawCount - 1; i >= 0; i--)
                ScheduledRenderTargetDrawsToRun[i]?.Invoke();

            ScheduledRenderTargetDrawsToRun.Clear();

#if DEBUG
            DebugScheduledRenderTargetDrawMs = ElapsedMilliseconds(phaseStartedTimestamp);
            phaseStartedTimestamp = Stopwatch.GetTimestamp();
#endif

            base.Draw(gameTime);

            Drawable.ResetTotalDrawnCount();

            // Draw the current game screen.
            ScreenManager.Draw(gameTime);

            TooltipManager.Draw(gameTime);

#if DEBUG
            DebugScreenDrawMs = ElapsedMilliseconds(phaseStartedTimestamp);
            phaseStartedTimestamp = Stopwatch.GetTimestamp();
#endif

            if (DrawGlobalUserInterface)
                GlobalUserInterface?.Draw(gameTime);

#if DEBUG
            DebugGlobalUiDrawMs = ElapsedMilliseconds(phaseStartedTimestamp);
            DebugDrawnDrawableCount = Drawable.TotalDrawn;
#endif

#if DEBUG
            TryEndBatch();
        }

        protected override void EndDraw()
        {
            if (IsReadyToUpdate)
            {
                var overlayStartedTimestamp = Stopwatch.GetTimestamp();
                DebugOverlay?.Draw(DebugDrawGameTime);
                var overlayDrawMs = ElapsedMilliseconds(overlayStartedTimestamp);

                PerformanceStats.RecordDrawTimings(DebugScheduledRenderTargetDrawMs, DebugScreenDrawMs, DebugGlobalUiDrawMs, overlayDrawMs,
                    ElapsedMilliseconds(DebugDrawStartedTimestamp), DebugDrawnDrawableCount);
            }

            if (SpriteBatch != null)
                TryEndBatch();

            base.EndDraw();
#endif
        }

#if DEBUG
        private static double ElapsedMilliseconds(long startedTimestamp) =>
            Stopwatch.GetElapsedTime(startedTimestamp).TotalMilliseconds;
#endif

        /// <summary>
        ///     Resets the backbuffer alpha channel to 1 (fully opaque).
        /// </summary>
        /// <param name="gameTime"></param>
        protected void ClearAlphaChannel(GameTime gameTime)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (alphaOneSprite.Width != WindowManager.VirtualScreen.X
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || alphaOneSprite.Height != WindowManager.VirtualScreen.Y)
                alphaOneSprite.Size = new ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);

            alphaOneSprite.Draw(gameTime);
        }
    }
}
