namespace Wobble.Platform.Linux
{
    public class SdlClipboard : Clipboard
    {
        public override string GetText() => GameBase.Game.GetClipboardText();

        public override void SetText(string selectedText) => GameBase.Game.SetClipboardText(selectedText);
    }
}