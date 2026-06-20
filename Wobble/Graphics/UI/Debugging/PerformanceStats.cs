using System;
using Microsoft.Xna.Framework;
using Wobble.Window;

namespace Wobble.Graphics.UI.Debugging
{
    public static class PerformanceStats
    {
        public static double FrameTimeMs { get; private set; }
        public static double UpdateTimeMs { get; private set; }
        public static double DrawTimeMs { get; private set; }
        public static double InputUpdateTimeMs { get; private set; }
        public static double ScreenUpdateTimeMs { get; private set; }
        public static double GlobalUiUpdateTimeMs { get; private set; }
        public static double AudioLogUpdateTimeMs { get; private set; }
        public static double ScheduledRenderTargetDrawTimeMs { get; private set; }
        public static double ScreenDrawTimeMs { get; private set; }
        public static double GlobalUiDrawTimeMs { get; private set; }
        public static double OverlayDrawTimeMs { get; private set; }

        public static double AverageFrameTimeMs { get; private set; }
        public static double AverageUpdateTimeMs { get; private set; }
        public static double AverageDrawTimeMs { get; private set; }

        public static int FrameRate { get; private set; }
        public static int UpdateRate { get; private set; }
        public static int DrawnDrawableCount { get; private set; }
        public static int ScheduledRenderTargetDrawCount { get; private set; }
        public static string CurrentScreenName { get; private set; } = "None";

        public static int SpriteTextPlusRefreshesPerSecond { get; private set; }
        public static int SpriteTextPlusCacheBuildsPerSecond { get; private set; }
        public static int SpriteTextPlusCachedDrawsPerSecond { get; private set; }
        public static int SpriteTextPlusUncachedDrawsPerSecond { get; private set; }

        public static long ManagedMemoryBytes { get; private set; }
        public static int Gen0Collections { get; private set; }
        public static int Gen1Collections { get; private set; }
        public static int Gen2Collections { get; private set; }
        public static int Gen0CollectionsDelta { get; private set; }
        public static int Gen1CollectionsDelta { get; private set; }
        public static int Gen2CollectionsDelta { get; private set; }

        public static int ImGuiVertexCount { get; private set; }
        public static int ImGuiIndexCount { get; private set; }

        private static double elapsedSampleMs;
        private static int frameCounter;
        private static int updateCounter;

        private static int spriteTextPlusRefreshes;
        private static int spriteTextPlusCacheBuilds;
        private static int spriteTextPlusCachedDraws;
        private static int spriteTextPlusUncachedDraws;

        private static int lastGen0Collections;
        private static int lastGen1Collections;
        private static int lastGen2Collections;

        public static void BeginUpdate(GameTime gameTime)
        {
            FrameTimeMs = gameTime.ElapsedGameTime.TotalMilliseconds;
            updateCounter++;
            elapsedSampleMs += FrameTimeMs;

            if (elapsedSampleMs < 1000)
                return;

            AverageFrameTimeMs = frameCounter == 0 ? 0 : elapsedSampleMs / frameCounter;
            AverageUpdateTimeMs = updateCounter == 0 ? 0 : AverageUpdateTimeMs;
            AverageDrawTimeMs = frameCounter == 0 ? 0 : AverageDrawTimeMs;

            FrameRate = frameCounter;
            UpdateRate = updateCounter;
            frameCounter = 0;
            updateCounter = 0;
            elapsedSampleMs -= 1000;

            SpriteTextPlusRefreshesPerSecond = spriteTextPlusRefreshes;
            SpriteTextPlusCacheBuildsPerSecond = spriteTextPlusCacheBuilds;
            SpriteTextPlusCachedDrawsPerSecond = spriteTextPlusCachedDraws;
            SpriteTextPlusUncachedDrawsPerSecond = spriteTextPlusUncachedDraws;

            spriteTextPlusRefreshes = 0;
            spriteTextPlusCacheBuilds = 0;
            spriteTextPlusCachedDraws = 0;
            spriteTextPlusUncachedDraws = 0;

            ManagedMemoryBytes = GC.GetTotalMemory(false);
            UpdateGcStats();
        }

        public static void RecordUpdateTimings(double inputUpdateMs, double screenUpdateMs, double globalUiUpdateMs,
            double audioLogUpdateMs, double updateMs)
        {
            InputUpdateTimeMs = inputUpdateMs;
            ScreenUpdateTimeMs = screenUpdateMs;
            GlobalUiUpdateTimeMs = globalUiUpdateMs;
            AudioLogUpdateTimeMs = audioLogUpdateMs;
            UpdateTimeMs = updateMs;
            AverageUpdateTimeMs = Smooth(AverageUpdateTimeMs, updateMs);
        }

        public static void BeginDraw(int scheduledRenderTargetDrawCount, string currentScreenName)
        {
            ScheduledRenderTargetDrawCount = scheduledRenderTargetDrawCount;
            CurrentScreenName = currentScreenName ?? "None";
        }

        public static void RecordDrawTimings(double scheduledRenderTargetDrawMs, double screenDrawMs, double globalUiDrawMs,
            double overlayDrawMs, double drawMs, int drawnDrawableCount)
        {
            ScheduledRenderTargetDrawTimeMs = scheduledRenderTargetDrawMs;
            ScreenDrawTimeMs = screenDrawMs;
            GlobalUiDrawTimeMs = globalUiDrawMs;
            OverlayDrawTimeMs = overlayDrawMs;
            DrawTimeMs = drawMs;
            DrawnDrawableCount = drawnDrawableCount;
            AverageDrawTimeMs = Smooth(AverageDrawTimeMs, drawMs);
            frameCounter++;
        }

        public static void RecordSpriteTextPlusRefresh() => spriteTextPlusRefreshes++;

        public static void RecordSpriteTextPlusCacheBuild() => spriteTextPlusCacheBuilds++;

        public static void RecordSpriteTextPlusDraw(bool cached)
        {
            if (cached)
                spriteTextPlusCachedDraws++;
            else
                spriteTextPlusUncachedDraws++;
        }

        public static void RecordImGuiDrawData(int vertexCount, int indexCount)
        {
            ImGuiVertexCount = vertexCount;
            ImGuiIndexCount = indexCount;
        }

        public static string FormatBytes(long bytes)
        {
            const double mb = 1024d * 1024d;
            return $"{bytes / mb:0.0} MB";
        }

        public static string BackBufferDescription =>
            $"{GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferWidth}x{GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferHeight}";

        public static string FramePacingDescription
        {
            get
            {
                if (GameBase.Game.Graphics.SynchronizeWithVerticalRetrace)
                    return "VSync";

                if (!GameBase.Game.IsFixedTimeStep)
                    return "Unlimited";

                var limit = (int)Math.Round(1d / GameBase.Game.TargetElapsedTime.TotalSeconds);
                return $"Limited ({limit} FPS)";
            }
        }

        public static string VirtualResolutionDescription =>
            $"{WindowManager.VirtualScreen.X:0}x{WindowManager.VirtualScreen.Y:0}";

        public static string ScreenScaleDescription =>
            $"{WindowManager.ScreenScale.X:0.00}x{WindowManager.ScreenScale.Y:0.00}";

        private static double Smooth(double previous, double current) => previous == 0 ? current : previous * 0.9 + current * 0.1;

        private static void UpdateGcStats()
        {
            Gen0Collections = GC.CollectionCount(0);
            Gen1Collections = GC.CollectionCount(1);
            Gen2Collections = GC.CollectionCount(2);

            Gen0CollectionsDelta = Gen0Collections - lastGen0Collections;
            Gen1CollectionsDelta = Gen1Collections - lastGen1Collections;
            Gen2CollectionsDelta = Gen2Collections - lastGen2Collections;

            lastGen0Collections = Gen0Collections;
            lastGen1Collections = Gen1Collections;
            lastGen2Collections = Gen2Collections;
        }
    }
}
