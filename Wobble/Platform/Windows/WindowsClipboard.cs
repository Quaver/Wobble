using TextCopy;

namespace Wobble.Platform.Windows
{
    public class WindowsClipboard : Clipboard
    {
        public override string GetText() => ClipboardService.GetText() ?? string.Empty;

        public override void SetText(string selectedText) => ClipboardService.SetText(selectedText);
    }
}
