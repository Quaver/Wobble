using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System;
using System.Linq;
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

        public override void RegisterFileAssociations(IReadOnlyDictionary<string, FileAssociation> associations,
            string applicationIconPath)
        {
            var applicationLocation = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, null);
            var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
                           Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share");
            var desktopFileName = "Quaver-file-handler.desktop";
            var desktopFilePath = Path.Combine(dataHome, $"applications/{desktopFileName}");
            var mimePackageDirectory = Path.Combine(dataHome, "mime/packages");
            var mimePackagePath = Path.Combine(mimePackageDirectory, "quaver-file-associations.xml");

            Directory.CreateDirectory(Path.GetDirectoryName(desktopFilePath));
            Directory.CreateDirectory(mimePackageDirectory);

            var mimeTypes = string.Join(";", associations.Values.Select(x => x.MimeType)) + ";";

            string[] desktopFileContents = {
                "[Desktop Entry]",
                "Name=Quaver",
                $"Exec=\"{applicationLocation}\" %f",
                $"Icon={applicationIconPath}",
                "Type=Application",
                "NoDisplay=true",
                $"MimeType={mimeTypes}"
            };
            File.WriteAllLines(desktopFilePath, desktopFileContents);

            var mimePackageContents = new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
                "<mime-info xmlns=\"http://www.freedesktop.org/standards/shared-mime-info\">"
            };

            foreach (var association in associations)
            {
                mimePackageContents.Add($"  <mime-type type=\"{association.Value.MimeType}\">");
                mimePackageContents.Add($"    <comment>{association.Value.FriendlyName}</comment>");
                mimePackageContents.Add($"    <glob pattern=\"*{association.Key}\"/>");
                mimePackageContents.Add("  </mime-type>");
            }

            mimePackageContents.Add("</mime-info>");
            File.WriteAllLines(mimePackagePath, mimePackageContents);

            TryRun("xdg-mime", "install", "--novendor", mimePackagePath);
            TryRun("update-mime-database", Path.Combine(dataHome, "mime"));

            foreach (var association in associations)
            {
                var iconName = association.Value.MimeType.Replace("/", "-");

                if (File.Exists(association.Value.IconPath))
                    TryRun("xdg-icon-resource", "install", "--novendor", "--context", "mimetypes", "--size", "256",
                        association.Value.IconPath, iconName);

                TryRun("xdg-mime", "default", desktopFileName, association.Value.MimeType);
            }
        }

        public override void EnableWindowsKey()
        {
        }

        public override void DisableWindowsKey()
        {
        }

        public override void ShowErrorMessage(string title, string message)
        {
            if (TryRunDialog("zenity", "--error", "--title", title, "--text", message))
                return;

            if (TryRunDialog("kdialog", "--error", message, "--title", title))
                return;

            TryRunDialog("xmessage", "-center", "-title", title, message);
        }

        private static void TryRun(string fileName, params string[] arguments)
        {
            try
            {
                var info = new ProcessStartInfo(fileName);

                foreach (var argument in arguments)
                    info.ArgumentList.Add(argument);

                var process = Process.Start(info);
                process?.WaitForExit();
            }
            catch (Exception)
            {
                // No matching XDG tool? Oh well.
            }
        }

        private static bool TryRunDialog(string fileName, params string[] arguments)
        {
            try
            {
                var info = new ProcessStartInfo(fileName);

                foreach (var argument in arguments)
                    info.ArgumentList.Add(argument);

                Process.Start(info)?.WaitForExit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
