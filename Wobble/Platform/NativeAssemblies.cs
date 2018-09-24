using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wobble.Platform
{
    internal static class NativeAssemblies
    {
        /// <summary>
        ///     The architecture of the CPU.
        /// </summary>
        internal static string Architecture => IntPtr.Size == 4 ? "x86" : "x64";

        /// <summary>
        ///     The base directory of the executable.
        /// </summary>
        internal static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     The directory of all native libs.
        /// </summary>
        internal static string NativeDirectory => $"{BaseDirectory}/{Architecture}/";

        /// <summary>
        ///     Copies native assemblies to the executing path
        /// </summary>
        internal static void Copy()
        {
            foreach (var file in Directory.GetFiles(NativeDirectory))
                File.Copy(file, Path.Combine(BaseDirectory, Path.GetFileName(file)));
        }
    }
}
