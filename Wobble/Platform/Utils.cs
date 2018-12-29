using System;
using System.IO;
using System.Runtime.InteropServices;
using Wobble.Platform.Linux;
using Wobble.Platform.Windows;

namespace Wobble.Platform
{
    public abstract class Utils
    {
        public static Utils NativeUtils
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return new WindowsUtils();
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return new LinuxUtils();
                }

                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Opens a native file manager highlighting a file.
        /// </summary>
        /// <param name="path">path to the file to highlight</param>
        public abstract void HighlightInFileManager(string path);

        /// <summary>
        ///     Opens a file or folder natively. Folders are opened with a native file manager and files are opened
        ///     according to the native associations.
        /// </summary>
        /// <param name="path">path to the file or folder to open</param>
        public abstract void OpenNatively(string path);
    }
}