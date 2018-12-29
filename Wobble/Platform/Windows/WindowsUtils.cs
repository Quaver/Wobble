using System.Diagnostics;

namespace Wobble.Platform.Windows
{
    public class WindowsUtils : Utils
    {
        public override void HighlightInFileManager(string path)
        {
            Process.Start("explorer.exe", "/select, \"" + path.Replace("/", "\\") + "\"");
        }

        public override void OpenNatively(string path)
        {
            Process.Start("explorer.exe", "\"" + path.Replace("/", "\\") + "\"");
        }
    }
}