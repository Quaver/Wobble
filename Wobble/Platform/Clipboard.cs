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

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (GameBase.Game.Window.GetType().Name == "SdlGameWindow")
                    {
                        return new SdlClipboard();
                    }
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return new OsxClipboard();

                return null;
            }
        }

        public abstract string GetText();

        public abstract void SetText(string selectedText);
    }
}
