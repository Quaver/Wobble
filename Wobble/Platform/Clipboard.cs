using System.Runtime.InteropServices;
using TextCopy;
using Wobble.Platform.Linux;

namespace Wobble.Platform
{
    public abstract class Clipboard
    {
        public static Clipboard NativeClipboard { get; } =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? (Clipboard)new SdlClipboard()
                : new TextCopyClipboard();

        public abstract string GetText();

        public abstract void SetText(string selectedText);

        private sealed class TextCopyClipboard : Clipboard
        {
            public override string GetText() => ClipboardService.GetText() ?? string.Empty;

            public override void SetText(string selectedText) => ClipboardService.SetText(selectedText);
        }
    }
}
