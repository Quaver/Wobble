// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Wobble.Platform.Linux
{
    public class NativeLibrary
    {
        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen(string library, LoadFlags flags);

        /// <summary>
        /// Loads a library with flags to use with dlopen. Uses <see cref="LoadFlags"/> for the flags
        ///
        /// Uses NATIVE_DLL_SEARCH_DIRECTORIES and then ld.so for library paths
        /// </summary>
        /// <param name="library">Full name of the library</param>
        /// <param name="flags">See 'man dlopen' for more information.</param>
        public static void Load(string library, LoadFlags flags)
        {
            var paths = (string)AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES");
            foreach (var path in paths.Split(':'))
            {
                if (dlopen(Path.Combine(path, library), flags) != IntPtr.Zero)
                    break;
            }
        }

        [Flags]
        public enum LoadFlags
        {
            RTLD_LAZY = 0x00001,
            RTLD_NOW = 0x00002,
            RTLD_BINDING_MASK = 0x00003,
            RTLD_NOLOAD = 0x00004,
            RTLD_DEEPBIND = 0x00008,
            RTLD_GLOBAL = 0x00100,
            RTLD_LOCAL = 0x00000,
            RTLD_NODELETE = 0x01000
        }
    }
}