using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

namespace Wobble.Graphics.UI.Debugging
{
    internal static class GraphicsRuntimeInfo
    {
        private delegate IntPtr GetCurrentVideoDriverDelegate();

        private static readonly string videoDriver = GetCurrentVideoDriver();

        public static string GraphicsBackend => PlatformInfo.GraphicsBackend.ToString();

        public static string GraphicsAdapter => GameBase.Game.GraphicsDevice.Adapter?.Description ?? "Unknown";

        public static string OperatingSystem => RuntimeInformation.OSDescription.Trim();

        public static string WindowSystem => videoDriver ?? GetSessionWindowSystem();

        public static string WaylandStatus
        {
            get
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "not applicable";

                if (string.Equals(videoDriver, "wayland", StringComparison.OrdinalIgnoreCase))
                    return "active";

                if (!string.IsNullOrWhiteSpace(videoDriver))
                    return $"inactive ({videoDriver} active)";

                return string.Equals(Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"), "wayland",
                    StringComparison.OrdinalIgnoreCase)
                    ? "session detected; SDL driver unknown"
                    : "inactive";
            }
        }

        private static string GetSessionWindowSystem()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "native";

            var session = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
            return string.IsNullOrWhiteSpace(session) ? "unknown" : $"{session} (session)";
        }

        private static string GetCurrentVideoDriver()
        {
            var libraryNames = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[] { "SDL2.dll" }
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? new[] { "libSDL2.dylib", "SDL2" }
                    : new[] { "libSDL2-2.0.so.0", "libSDL2.so", "SDL2" };

            foreach (var libraryName in libraryNames)
            {
                if (!NativeLibrary.TryLoad(libraryName, out var library))
                    continue;

                try
                {
                    if (!NativeLibrary.TryGetExport(library, "SDL_GetCurrentVideoDriver", out var function))
                        continue;

                    var getCurrentVideoDriver = Marshal.GetDelegateForFunctionPointer<GetCurrentVideoDriverDelegate>(function);
                    var driver = getCurrentVideoDriver();
                    return driver == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(driver);
                }
                catch
                {
                    // Diagnostics should never prevent the game from starting.
                }
                finally
                {
                    NativeLibrary.Free(library);
                }
            }

            return null;
        }
    }
}
