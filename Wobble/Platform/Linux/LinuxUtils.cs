using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System;
using Wobble.Logging;

namespace Wobble.Platform.Linux
{
    public class LinuxUtils : Utils
    {
        public override void HighlightInFileManager(string path)
        {
            try
            {
                // Try launching nautilus (GNOME's file manager) first as it can highlight a file by path.
                var info = new ProcessStartInfo("nautilus")
                {
                    ArgumentList = { path }
                };
                Process.Start(info);
            }
            catch (Exception)
            {
                // There isn't really a standard way of doing this on Linux, so fall back to just opening the containing
                // folder.
                OpenNatively(Path.GetDirectoryName(path));
            }
        }

        public override void OpenNatively(string path)
        {
            try
            {
                // Try opening via xdg-open.
                var info = new ProcessStartInfo("xdg-open")
                {
                    ArgumentList = { path }
                };
                Process.Start(info);
            }
            catch (Exception)
            {
                // No xdg-open? Oh well.
            }
        }

        public override void RegisterURIScheme(string scheme, string friendlyName)
        {
            // This returns Something.dll, on Linux the published executable is usually called Something.
            var applicationLocation = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, null);

            // Just "friendlyName" is taken by Steam's own shortcut to the game.
            friendlyName = friendlyName + "-scheme-handler";

            var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share");
            var desktopFilePath = Path.Combine(dataHome, $"applications/{friendlyName}.desktop");

            // Make a .desktop file.
            string[] contents = {
                "[Desktop Entry]",
                $"Name={friendlyName}",
                $"Exec={applicationLocation} %u",
                "Type=Application",
                "NoDisplay=true",
                $"MimeType=x-scheme-handler/{scheme};"
            };
            File.WriteAllLines(desktopFilePath, contents);

            // Register it as the handler.
            try
            {
                Process.Start("xdg-mime", $"default {friendlyName}.desktop x-scheme-handler/{scheme}");
            }
            catch (Exception)
            {
                // No xdg-mime? Oh well.
            }
        }

        public override void EnableWindowsKey()
        {
        }

        public override void DisableWindowsKey()
        {
        }
    }
}