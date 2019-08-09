using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Audio;
using Wobble.Discord;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Input;
using Wobble.IO;
using Wobble.Logging;
using Wobble.Platform;
using Wobble.Platform.Linux;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble
{
    /// <summary>
    ///    The main game class. This will handle all the lower level details that should not
    ///    be worried about. It takes care of the majority of the low level and boilerplate stuff, so that
    ///    all you have to do is just write the game.
    /// </summary>
    public abstract class WobbleGame : Game
    {
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
        ///     The amount of time elapsed since the previous frame in Milliseconds.
        /// </summary>
        public double TimeSinceLastFrame { get; private set; }

        /// <summary>
        ///     The amount of time the game has been running.
        /// </summary>
        public long TimeRunning { get; private set; }

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
        public List<Action> ScheduledRenderTargetDraws { get; } = new List<Action>();

        /// <summary>
        ///     Creates a game with embedded resources as a content manager.
        /// </summary>
        protected WobbleGame()
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Required for libbass_fx.so to load properly on Linux and not crash (see https://github.com/ppy/osu/issues/2852).
                NativeLibrary.Load("libbass.so", NativeLibrary.LoadFlags.RTLD_LAZY | NativeLibrary.LoadFlags.RTLD_GLOBAL);
            }
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
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            WobbleAssets.Dispose();
            WindowManager.UnHookEvents();
            AudioManager.Dispose();
            DiscordManager.Dispose();
            BitmapFontFactory.Dispose();
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

            // Update the time since the last frame and the game's clock.
            TimeSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;
            TimeRunning += (long) gameTime.ElapsedGameTime.TotalMilliseconds;

            Drawable.ResetTotalDrawnCount();

            // Keep the window updated with the current resolution.
            WindowManager.Update();
            MouseManager.Update();
            KeyboardManager.Update();
            ScreenManager.Update(gameTime);
            AudioManager.Update(gameTime);

            // Update the global sprite container
            GlobalUserInterface.Update(gameTime);

            // Keep the RPC client up-to-date.
            DiscordManager.Client?.Invoke();
            LogManager.Update(gameTime);
            Logger.Update();

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

            for (var i = ScheduledRenderTargetDraws.Count - 1; i >= 0; i--)
            {
                ScheduledRenderTargetDraws[i]?.Invoke();
                ScheduledRenderTargetDraws.Remove(ScheduledRenderTargetDraws[i]);
            }

            base.Draw(gameTime);

            // Draw the current game screen.
            ScreenManager.Draw(gameTime);
        }
    }
}