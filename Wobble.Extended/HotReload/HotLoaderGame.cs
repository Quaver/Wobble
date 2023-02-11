using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using Wobble.Assets;
using Wobble.Extended.HotReload.Screens;
using Wobble.Screens;

namespace Wobble.Extended.HotReload
{
    public abstract class HotLoaderGame : WobbleGame
    {
        protected override bool IsReadyToUpdate { get; set; }

        /// <summary>
        /// </summary>
        public HotLoader HotLoader { get; }

        /// <summary>
        /// </summary>
        public HotLoaderScreen HotLoaderScreen { get; protected set; }

        public static BitmapFont Font { get; set; }

        public HotLoaderGame(HotLoader hl, bool preferWayland = false) : base(preferWayland)
        {
            HotLoader = hl;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            HotLoaderScreen = InitializeHotLoaderScreen();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            ScreenManager.ChangeScreen(HotLoaderScreen);

            HotLoader.CompileProject();
            HotLoader.LoadDll();

            IsReadyToUpdate = true;
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        protected abstract HotLoaderScreen InitializeHotLoaderScreen();
    }
}
