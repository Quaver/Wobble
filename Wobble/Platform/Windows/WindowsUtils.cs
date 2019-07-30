using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

        public override void RegisterURIScheme(string scheme, string friendlyName)
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + scheme))
            {
                // This returns Something.dll, on Windows the published executable is usually called Something.exe.
                var applicationLocation = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, "exe");

                key.SetValue("", "URL:" + friendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }
    }
}