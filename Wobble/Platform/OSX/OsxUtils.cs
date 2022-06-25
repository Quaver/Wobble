using System.Diagnostics;

namespace Wobble.Platform.OSX
{
    public class OsxUtils : Utils
    {
        public override void HighlightInFileManager(string path)
        {
            var info = new ProcessStartInfo("open")
            {
                ArgumentList = {"-R", path}
            };
            Process.Start(info);
        }

        public override void OpenNatively(string path)
        {
            var info = new ProcessStartInfo("open")
            {
                ArgumentList = {"-R", path}
            };
            Process.Start(info);
        }

        public override void RegisterURIScheme(string scheme, string friendlyName)
        {
        }

        public override void EnableWindowsKey()
        {
        }

        public override void DisableWindowsKey()
        {
        }
    }
}
