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
using System.Runtime.InteropServices;

namespace Wobble.Platform.Linux
{
    public class SdlClipboard : Clipboard
    {
        private const string lib = "libSDL2-2.0.so.0";

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_free", ExactSpelling = true)]
        private static extern void SDL_free(IntPtr ptr);

        /// <returns>Returns the clipboard text on success or <see cref="IntPtr.Zero"/> on failure.</returns>
        [DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetClipboardText", ExactSpelling = true)]
        private static extern IntPtr SDL_GetClipboardText();

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetClipboardText", ExactSpelling = true)]
        private static extern int SDL_SetClipboardText(string text);

        public override string GetText()
        {
            var ptrToText = SDL_GetClipboardText();
            var text = Marshal.PtrToStringAnsi(ptrToText);
            SDL_free(ptrToText);
            return text;
        }

        public override void SetText(string selectedText)
        {
            SDL_SetClipboardText(selectedText);
        }
    }
}