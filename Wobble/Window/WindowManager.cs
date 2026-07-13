using System;
using Microsoft.Xna.Framework;
using Wobble.Graphics;

namespace Wobble.Window
{
    public static class WindowManager
    {
        /// <summary>
        /// </summary>
        private static Vector3 ScalingFactor { get; set; }

        /// <summary>
        ///     The back buffer's width
        /// </summary>
        private static int PreferredBackBufferWidth { get; set; }

        /// <summary>
        ///     The back buffer's height
        /// </summary>
        private static int PreferredBackBufferHeight { get; set; }

        /// <summary>
        ///     Whether a user-driven resize needs to be applied on the next update.
        /// </summary>
        private static bool ApplyChangesQueued { get; set; }

        /// <summary>
        ///     Prevents an ApplyChanges-triggered resize notification from queuing another swapchain recreation.
        /// </summary>
        private static bool IsApplyingChanges { get; set; }

        /// <summary>
        ///     The base resolution to draw at for sizes, positions, and scaling.
        /// </summary>
        public static Vector2 BaseResolution { get; private set; } = new Vector2(1366, 768);

        /// <summary>
        ///     The virtual screen size. The default is 1280x720. This is the resolution at which the
        ///     game will render at.
        /// </summary>
        public static Vector2 VirtualScreen { get; private set; } = new Vector2(1366, 768);

        /// <summary>
        ///     The ratio of <see cref="BaseResolution"/> to <see cref="BaseToVirtualRatio"/>
        /// </summary>
        public static float BaseToVirtualRatio => VirtualScreen.Y / BaseResolution.Y;

        /// <summary>
        ///     The width of the virtual screen.
        /// </summary>
        public static float Width => VirtualScreen.X;

        /// <summary>
        ///     The height of the virtual screen.
        /// </summary>
        public static float Height => VirtualScreen.Y;

        /// <summary>
        ///     Rectangle object for the screen size.
        /// </summary>
        public static Rectangle Rectangle => new Rectangle(0, 0, (int)VirtualScreen.X, (int)VirtualScreen.Y);

        /// <summary>
        ///     The result of merging VirtualScreen with WindowScreen
        /// </summary>
        public static Vector2 ScreenScale { get; private set; }

        /// <summary>
        ///     The aspect ratio of the screen.
        /// </summary>
        public static Vector2 ScreenAspectRatio { get; private set; }

        /// <summary>
        ///     The scale used for beginning the SpriteBatch.
        /// </summary>
        public static Matrix Scale { get; private set; }

        /// <summary>
        ///     Event raised when the resolution of the window has changed.
        /// </summary>
        public static event EventHandler<WindowResolutionChangedEventArgs> ResolutionChanged;

        /// <summary>
        ///     Event raised when <see cref="VirtualScreen"/> has been changed
        /// </summary>
        public static event EventHandler<WindowVirtualScreenSizeChangedEventArgs> VirtualScreenSizeChanged;

        /// <summary>
        ///     Called every frame. This will continuously update our screen scale & matrix
        ///     based on the back buffer's current resolution.
        /// </summary>
        public static void Update()
        {
            // Grab the GraphicsDeviceManager from the game instance.
            var gdm = GameBase.Game.Graphics;

            if (gdm == null)
                throw new ArgumentNullException($"GraphicsDeviceManager");

            ApplyQueuedChanges(gdm);

            // Grab the back buffer's dimensions.
            PreferredBackBufferWidth = gdm.PreferredBackBufferWidth;
            PreferredBackBufferHeight = gdm.PreferredBackBufferHeight;

            // Calculate screen scale and aspect ratio.
            ScreenScale = new Vector2(PreferredBackBufferWidth / VirtualScreen.X, PreferredBackBufferHeight / VirtualScreen.Y);
            ScreenAspectRatio = new Vector2(ScreenScale.X / ScreenScale.Y);

            // Create the matrix at which we'll be drawing sprites.
            ScalingFactor = new Vector3(ScreenScale.X, ScreenScale.Y, 1);
            Scale = Matrix.CreateScale(ScalingFactor);

        }

        /// <summary>
        ///     Unhooks all events from the WindowManager.
        /// </summary>
        public static void UnHookEvents()
        {
            ResolutionChanged = null;
            VirtualScreenSizeChanged = null;
            ApplyChangesQueued = false;
            IsApplyingChanges = false;
        }

        /// <summary>
        ///     Changes the size of the virtual screen to be used.
        /// </summary>
        /// <param name="newScreenSize"></param>
        public static void ChangeVirtualScreenSize(Vector2 newScreenSize)
        {
            VirtualScreen = newScreenSize;
            VirtualScreenSizeChanged?.Invoke(typeof(WindowManager), new WindowVirtualScreenSizeChangedEventArgs(VirtualScreen));
        }

        /// <summary>
        ///     Changes the base resolution of the game
        /// </summary>
        /// <param name="size"></param>
        public static void ChangeBaseResolution(Vector2 size) => BaseResolution = size;

        /// <summary>
        ///     Determines the draw scaling.
        ///     Used to make the mouse scale correctly according to the virtual resolution,
        ///     no matter the actual resolution.
        ///
        ///     Example: 1920x1080 applied to 1280x800: new Vector2(1.5f, 1,35f)
        /// </summary>
        /// <returns></returns>
        public static Vector2 DetermineDrawScaling()
        {
            var x = PreferredBackBufferWidth / VirtualScreen.X;
            var y = PreferredBackBufferHeight / VirtualScreen.Y;
            return new Vector2(x, y);
        }

        /// <summary>
        ///     Changes the resolution of the screen.
        /// </summary>
        /// <param name="resolution">The resolution to change to</param>
        public static void ChangeScreenResolution(Point resolution)
        {
            var gdm = GameBase.Game.Graphics;

            var oldResolution = new Point(gdm.PreferredBackBufferWidth, gdm.PreferredBackBufferHeight);

            gdm.PreferredBackBufferWidth = resolution.X;
            gdm.PreferredBackBufferHeight = resolution.Y;
            ApplyChangesQueued = false;
            ApplyChanges(gdm);

            // Raise an event to let everyone know that the window has changed.
            ResolutionChanged?.Invoke(typeof(WindowManager), new WindowResolutionChangedEventArgs(resolution, oldResolution));
        }

        /// <summary>
        ///     When the window size changes, update the PreferredBackBuffer's size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnClientSizeChanged(object sender, EventArgs e)
        {
            if (IsApplyingChanges)
                return;

            UpdateBackBufferSize();
        }

        /// <summary>
        ///     Updates the size of the back buffer to the current window's size.
        /// </summary>
        private static void UpdateBackBufferSize()
        {
            var bounds = GameBase.Game.Window.ClientBounds;

            // Minimized windows can report a zero-sized drawable. Keep the current swapchain until restored.
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            var graphics = GameBase.Game.Graphics;

            if (graphics.PreferredBackBufferWidth == bounds.Width &&
                graphics.PreferredBackBufferHeight == bounds.Height)
                return;

            graphics.PreferredBackBufferWidth = bounds.Width;
            graphics.PreferredBackBufferHeight = bounds.Height;
            ApplyChangesQueued = true;
        }

        private static void ApplyQueuedChanges(GraphicsDeviceManager graphics)
        {
            if (!ApplyChangesQueued)
                return;

            if (graphics.PreferredBackBufferWidth <= 0 || graphics.PreferredBackBufferHeight <= 0)
                return;

            ApplyChangesQueued = false;
            ApplyChanges(graphics);
        }

        private static void ApplyChanges(GraphicsDeviceManager graphics)
        {
            if (IsApplyingChanges)
                return;

            try
            {
                IsApplyingChanges = true;
                graphics.ApplyChanges();
            }
            finally
            {
                IsApplyingChanges = false;
            }
        }
    }
}
