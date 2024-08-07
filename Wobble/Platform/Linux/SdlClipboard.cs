namespace Wobble.Platform.Linux
{
    public class SdlClipboard : Clipboard
    {
        public override string GetText() => GameBase.Game.GetClipboardText().TrimEnd('\0');

        public override void SetText(string selectedText) => GameBase.Game.SetClipboardText(selectedText.TrimEnd('\0'));
    }
}