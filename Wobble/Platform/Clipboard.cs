using System;
using System.Runtime.InteropServices;
using Wobble.Platform.Linux;
using Wobble.Platform.OSX;
using Wobble.Platform.Windows;

namespace Wobble.Platform
{
    public abstract class Clipboard
    {
        public static Clipboard NativeClipboard { get; } = GetClipboard();

        public abstract string GetText();

        public abstract void SetText(string selectedText);

        static Clipboard GetClipboard()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsClipboard();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return null;

            var xdg = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
            var display = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            var isWayland = StringComparer.OrdinalIgnoreCase.Equals(xdg, "wayland") || display is object;

            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && isWayland ?
                (Clipboard)new WaylandClipboard() :
                GameBase.Game.Window.GetType().Name is "SdlGameWindow" ? new SdlClipboard() : null;
        }
    }
}
