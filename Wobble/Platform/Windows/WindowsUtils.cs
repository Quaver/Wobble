using Microsoft.Win32;
using System.Collections.Generic;
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

        public override void RegisterFileAssociations(IReadOnlyDictionary<string, FileAssociation> associations,
            string applicationIconPath)
        {
            foreach (var association in associations)
            {
                var extension = association.Key.StartsWith(".") ? association.Key : "." + association.Key;
                var progId = association.Value.ProgId;

                using (var extensionKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\" + extension))
                {
                    extensionKey.SetValue("", progId);

                    using (var openWithProgIds = extensionKey.CreateSubKey("OpenWithProgids"))
                        openWithProgIds.SetValue(progId, new byte[0], RegistryValueKind.None);
                }

                using (var progIdKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\" + progId))
                {
                    // This returns Something.dll, on Windows the published executable is usually called Something.exe.
                    var applicationLocation = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, "exe");

                    progIdKey.SetValue("", association.Value.FriendlyName);

                    using (var defaultIcon = progIdKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIcon.SetValue("", "\"" + association.Value.IconPath + "\",0");
                    }

                    using (var commandKey = progIdKey.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                    }
                }
            }
        }

        public override void EnableWindowsKey() => WindowsKey.EnableWindowsKey();

        public override void DisableWindowsKey() => WindowsKey.DisableWindowsKey();
    }
}
