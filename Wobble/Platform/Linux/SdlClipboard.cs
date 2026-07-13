using TextCopy;

namespace Wobble.Platform.Linux
{
    public class SdlClipboard : Clipboard
    {
        public override string GetText() => (ClipboardService.GetText() ?? string.Empty).TrimEnd('\0');

        public override void SetText(string selectedText) => ClipboardService.SetText(selectedText.TrimEnd('\0'));
    }
}
