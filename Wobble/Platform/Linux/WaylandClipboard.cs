// SPDX-License-Identifier: MPL-2.0
namespace Wobble.Platform.Linux
{
    public class WaylandClipboard : Clipboard
    {
        public override string GetText() => TextCopy.ClipboardService.GetText();

        // SdlClipboard causes a null byte to be appended to the clipboard.
        public override void SetText(string selectedText) => TextCopy.ClipboardService.SetText(selectedText);
    }
}
