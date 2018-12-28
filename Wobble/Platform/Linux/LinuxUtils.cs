using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Wobble.Platform.Linux
{
    public class LinuxUtils : Utils
    {
        public override void HighlightInFileManager(string path)
        {
            // There isn't really a standard way of doing this on Linux, so fall back to just opening the containing
            // folder.
            OpenNatively(Path.GetDirectoryName(path));
        }

        public override void OpenNatively(string path)
        {
            try
            {
                // Try opening via xdg-open.
                Process.Start("xdg-open", path);
            }
            catch (Win32Exception)
            {
                // No xdg-open? Oh well.
            }
        }
    }
}