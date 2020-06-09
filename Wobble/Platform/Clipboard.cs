using System;
using System.Runtime.InteropServices;
using Wobble.Platform.Linux;
using Wobble.Platform.OSX;
using Wobble.Platform.Windows;

namespace Wobble.Platform
{
    public abstract class Clipboard
    {
        public static Clipboard NativeClipboard
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return new WindowsClipboard();
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (GameBase.Game.Window.GetType().Name == "SdlGameWindow")
                    {
                        return new SdlClipboard();
                    }
                }

                return null;
            }
        }

        public abstract string GetText();

        public abstract void SetText(string selectedText);
    }
}
