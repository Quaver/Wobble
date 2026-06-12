using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Wobble.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsUtils : Utils
    {
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

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

                    using (var defaultIcon = extensionKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIcon.SetValue("", "\"" + association.Value.IconPath + "\",0");
                    }

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

            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        public override void EnableWindowsKey() => WindowsKey.EnableWindowsKey();

        public override void DisableWindowsKey() => WindowsKey.DisableWindowsKey();

        public override void ShowErrorMessage(string title, string message)
            => MessageBox(IntPtr.Zero, message, title, 0x00000010);

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
}
