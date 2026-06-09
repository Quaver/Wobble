using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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
            var bundlePath = GetAppBundlePath();

            if (bundlePath == null)
                return;

            var infoPlistPath = Path.Combine(bundlePath, "Contents", "Info.plist");
            var resourcesPath = Path.Combine(bundlePath, "Contents", "Resources");

            if (!File.Exists(infoPlistPath))
                return;

            Directory.CreateDirectory(resourcesPath);

            foreach (var association in associations.Values)
            {
                if (File.Exists(association.IconPath))
                    File.Copy(association.IconPath, Path.Combine(resourcesPath, Path.GetFileName(association.IconPath)), true);
            }

            if (File.Exists(applicationIconPath))
                File.Copy(applicationIconPath, Path.Combine(resourcesPath, Path.GetFileName(applicationIconPath)), true);

            var plist = XDocument.Load(infoPlistPath);
            var dict = plist.Root?.Element("dict");

            if (dict == null)
                return;

            RemovePlistKey(dict, "CFBundleIconFile");
            RemovePlistKey(dict, "CFBundleDocumentTypes");
            RemovePlistKey(dict, "UTExportedTypeDeclarations");

            dict.Add(new XElement("key", "CFBundleIconFile"));
            dict.Add(new XElement("string", Path.GetFileName(applicationIconPath)));

            dict.Add(new XElement("key", "CFBundleDocumentTypes"));
            dict.Add(CreateDocumentTypes(associations));

            dict.Add(new XElement("key", "UTExportedTypeDeclarations"));
            dict.Add(CreateExportedTypeDeclarations(associations));

            plist.Save(infoPlistPath);
        }

        public override void EnableWindowsKey()
        {
        }

        public override void DisableWindowsKey()
        {
        }

        private static string GetAppBundlePath()
        {
            var directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            while (!string.IsNullOrEmpty(directory))
            {
                if (directory.EndsWith(".app"))
                    return directory;

                directory = Path.GetDirectoryName(directory);
            }

            return null;
        }

        private static XElement CreateDocumentTypes(IReadOnlyDictionary<string, FileAssociation> associations)
        {
            var array = new XElement("array");

            foreach (var association in associations)
            {
                var extension = association.Key.TrimStart('.');
                var typeIdentifier = GetUniformTypeIdentifier(extension);

                array.Add(new XElement("dict",
                    new XElement("key", "CFBundleTypeExtensions"),
                    new XElement("array", new XElement("string", extension)),
                    new XElement("key", "CFBundleTypeIconFile"),
                    new XElement("string", Path.GetFileName(association.Value.IconPath)),
                    new XElement("key", "CFBundleTypeName"),
                    new XElement("string", association.Value.FriendlyName),
                    new XElement("key", "CFBundleTypeRole"),
                    new XElement("string", "Viewer"),
                    new XElement("key", "LSHandlerRank"),
                    new XElement("string", "Owner"),
                    new XElement("key", "LSItemContentTypes"),
                    new XElement("array", new XElement("string", typeIdentifier))));
            }

            return array;
        }

        private static XElement CreateExportedTypeDeclarations(IReadOnlyDictionary<string, FileAssociation> associations)
        {
            var array = new XElement("array");

            foreach (var association in associations)
            {
                var extension = association.Key.TrimStart('.');
                var typeIdentifier = GetUniformTypeIdentifier(extension);

                array.Add(new XElement("dict",
                    new XElement("key", "UTTypeIdentifier"),
                    new XElement("string", typeIdentifier),
                    new XElement("key", "UTTypeDescription"),
                    new XElement("string", association.Value.FriendlyName),
                    new XElement("key", "UTTypeConformsTo"),
                    new XElement("array", new XElement("string", "public.data")),
                    new XElement("key", "UTTypeTagSpecification"),
                    new XElement("dict",
                        new XElement("key", "public.filename-extension"),
                        new XElement("array", new XElement("string", extension)),
                        new XElement("key", "public.mime-type"),
                        new XElement("string", association.Value.MimeType))));
            }

            return array;
        }

        private static string GetUniformTypeIdentifier(string extension) => $"com.quavergame.{extension}";

        private static void RemovePlistKey(XElement dict, string key)
        {
            var keyElement = dict.Elements("key").FirstOrDefault(x => x.Value == key);

            if (keyElement == null)
                return;

            var valueElement = keyElement.ElementsAfterSelf().FirstOrDefault();

            keyElement.Remove();
            valueElement?.Remove();
        }
    }
}
