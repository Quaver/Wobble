namespace Wobble.Platform.Linux
{
    public class SdlClipboard : Clipboard
    {
        public override string GetText() => TextCopy.ClipboardService.GetText().TrimEnd('\0');

        public override void SetText(string selectedText) =>
            TextCopy.ClipboardService.SetText(selectedText.TrimEnd('\0'));
    }
}
