using TextCopy;

namespace Wobble.Platform
{
    public abstract class Clipboard
    {
        public static Clipboard NativeClipboard { get; } = new TextCopyClipboard();

        public abstract string GetText();

        public abstract void SetText(string selectedText);

        private sealed class TextCopyClipboard : Clipboard
        {
            public override string GetText() => ClipboardService.GetText() ?? string.Empty;

            public override void SetText(string selectedText) => ClipboardService.SetText(selectedText);
        }
    }
}
