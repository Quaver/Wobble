using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Extended.HotReload.Screens.UI;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;

namespace Wobble.Extended.HotReload.Screens
{
    public class HotLoaderScreenView : ScreenView
    {
        /// <summary>
        ///     The screens that are ready to be hotloaded
        /// </summary>
        private Dictionary<string, Type> TestScreens { get; }

        /// <summary>
        /// </summary>
        private CompilingNotification CompilingNotification { get; }

        /// <summary>
        /// </summary>
        public HotLoader HotLoader { get; }

        /// <summary>
        /// </summary>
        private HotLoadingBrowser Browser { get; }

        /// <summary>
        /// </summary>
        private bool BrowserActive { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="testScreens"></param>
        public HotLoaderScreenView(Screen screen, Dictionary<string, Type> testScreens) : base(screen)
        {
            TestScreens = testScreens;

            CompilingNotification = new CompilingNotification()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            Browser = new HotLoadingBrowser(this, testScreens) { Parent = Container };

            var game = (HotLoaderGame)GameBase.Game;
            HotLoader = game.HotLoader;

            Logger.Debug("Press the Tilde (~) key to open/close the test browser", LogType.Runtime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemTilde))
            {
                BrowserActive = !BrowserActive;
                Browser?.ClearAnimations();
                Browser?.MoveToX(BrowserActive ? 0 : -Browser.Width - 10, Easing.OutQuint, 400);
            }

            HandleCompilingNotificationAnimations(gameTime);

            if (!HotLoader.IsCompiling && !HotLoader.CompilationFailed)
                HotLoader?.Update(gameTime);

            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);

            HotLoader?.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleCompilingNotificationAnimations(GameTime gameTime)
        {
            var targetAlpha = HotLoader.IsCompiling || HotLoader.CompilationFailed ? 1 : 0;

            CompilingNotification.Alpha = MathHelper.Lerp(CompilingNotification.Alpha, targetAlpha,
                (float)Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            if (!HotLoader.IsCompiling && HotLoader.CompilationFailed && CompilingNotification.Text.Text.Contains("re-compiled"))
                CompilingNotification.SetCompilationFailedText();
            else if (HotLoader.IsCompiling && CompilingNotification.Text.Text.Contains("Failed"))
                CompilingNotification.SetCompilingText();
        }

        /// <summary>
        ///     Changes to a new screen in the browser
        /// </summary>
        /// <param name="t"></param>
        public void ChangeScreen(Type t)
        {
            foreach (Type type in HotLoader.Asm.GetExportedTypes())
            {
                if (type.FullName == t.FullName)
                {
                    // We found our gamelogic type, set our dynamic types logic, and state
                    var oldScreen = HotLoader.Screen;
                    oldScreen?.Destroy();

                    HotLoader.Screen = Activator.CreateInstance(t);
                    break;
                }
            }
        }
    }
}
