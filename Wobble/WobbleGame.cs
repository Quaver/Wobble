using System;
using System.Collections.Generic;
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
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
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
        public long TimeRunning => (long) TimeRunningPrecise;

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
        public List<Action<GameTime>> ScheduledRenderTargetDraws { get; } = new List<Action<GameTime>>();

        /// <summary>
        ///     The sprite used for clearing the alpha channel. Its alpha must be 1 (fully opaque) and its color does not matter.
        /// </summary>
        private readonly Sprite alphaOneSprite;

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
            TimeRunningPrecise += gameTime.ElapsedGameTime.TotalMilliseconds;

            Drawable.ResetTotalDrawnCount();

            // Keep the window updated with the current resolution.
            WindowManager.Update();
            MouseManager.Update();
            KeyboardManager.Update();
            JoystickManager.Update();
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

            var hasScheduledRenderTargetDraws = ScheduledRenderTargetDraws.Count > 0;

            for (var i = ScheduledRenderTargetDraws.Count - 1; i >= 0; i--)
            {
                ScheduledRenderTargetDraws[i]?.Invoke(gameTime);
                ScheduledRenderTargetDraws.Remove(ScheduledRenderTargetDraws[i]);
            }

            if (hasScheduledRenderTargetDraws)
                GraphicsDevice.SetRenderTarget(null);

            base.Draw(gameTime);

            // Draw the current game screen.
            ScreenManager.Draw(gameTime);
        }

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
