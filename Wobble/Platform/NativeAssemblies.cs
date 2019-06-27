using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wobble.Logging;

namespace Wobble.Platform
{
    public static class NativeAssemblies
    {
        /// <summary>
        ///     The architecture of the CPU.
        /// </summary>
        public static string Architecture => IntPtr.Size == 4 ? "x86" : "x64";

        /// <summary>
        ///     The base directory of the executable.
        /// </summary>
        public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     The directory of all native libs.
        /// </summary>
        public static string NativeDirectory => $"{BaseDirectory}/{Architecture}/";

        /// <summary>
        ///     Copies native assemblies to the executing path
        /// </summary>
        public static void Copy()
        {
            foreach (var file in Directory.GetFiles(NativeDirectory))
            {
                try
                {
                    var path = Path.Combine(BaseDirectory, Path.GetFileName(file));

                    if (!File.Exists(path))
                        File.Copy(file, path, true);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }
    }
}
