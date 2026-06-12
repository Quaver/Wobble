using System.Collections.Generic;
using System.Diagnostics;

namespace Wobble.Platform.OSX
{
    public class OsxUtils : Utils
    {
        public override void HighlightInFileManager(string path)
        {
            var info = new ProcessStartInfo("open")
            {
                ArgumentList = { "-R", path }
            };
            Process.Start(info);
        }

        public override void OpenNatively(string path)
        {
            var info = new ProcessStartInfo("open")
            {
                ArgumentList = { "-R", path }
            };
            Process.Start(info);
        }

        public override void RegisterURIScheme(string scheme, string friendlyName)
        {
        }

        public override void RegisterFileAssociations(IReadOnlyDictionary<string, FileAssociation> associations,
            string applicationIconPath)
        {
        }

        public override void EnableWindowsKey()
        {
        }

        public override void DisableWindowsKey()
        {
        }

        public override void ShowErrorMessage(string title, string message)
        {
            var info = new ProcessStartInfo("osascript")
            {
                ArgumentList =
                {
                    "-e",
                    $"display alert {QuoteAppleScript(title)} message {QuoteAppleScript(message)} as critical"
                }
            };

            Process.Start(info)?.WaitForExit();
        }

        private static string QuoteAppleScript(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
