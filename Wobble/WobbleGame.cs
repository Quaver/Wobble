using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Audio;
using Wobble.Input;
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
        ///     The singleton instance of the game.
        /// </summary>
        // ReSharper disable once InconsistentNaming (What?)
        private static WobbleGame _instance { get; set; }
        public static WobbleGame Instance
        {
            get => _instance;
            set
            {
                if (_instance != null)
                    throw new InvalidOperationException("There can only ever be one game in existence.");

                _instance = value;
            }
        }

        /// <summary> 
        /// </summary>
        public GraphicsDeviceManager Graphics { get; }

        /// <summary>
       ///    Used for drawing textures
       /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        ///     The amount of time elapsed since the previous frame.
        /// </summary>
        public double TimeSinceLastFrame { get; private set; }

        /// <summary>
        ///     The amount of time the game has been running.
        /// </summary>
        public int TimeRunning { get; private set; }

        /// <summary>
        ///     Creates a game with embedded resources as a content manager.
        /// </summary>
        protected WobbleGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Instance = this;
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            AudioEngine.Initialize();
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
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            WindowManager.UnHookEvents();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update the time since the last frame and the game's clock.
            TimeSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;
            TimeRunning = gameTime.TotalGameTime.Milliseconds;
            
            // Keep the window updated with the current resolution.
            WindowManager.Update();
            
            // Keep the current and previous mouse/keyboard states up-to-date.
            MouseManager.Update();
            KeyboardManager.Update();
            
            // Update the current game screen.
            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Draw the current game screen.
            ScreenManager.Draw(gameTime);
        }
    }
}
