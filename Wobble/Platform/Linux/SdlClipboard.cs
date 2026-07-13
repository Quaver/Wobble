using System;
using System.Runtime.InteropServices;

namespace Wobble.Platform.Linux
{
    public class SdlClipboard : Clipboard
    {
        private const string NativeLibrary = "mgruntime";

        public override string GetText()
        {
            var text = SdlGetClipboardText();

            if (text == IntPtr.Zero)
                return string.Empty;

            try
            {
                return (Marshal.PtrToStringUTF8(text) ?? string.Empty).TrimEnd('\0');
            }
            finally
            {
                SdlFree(text);
            }
        }

        public override void SetText(string selectedText) => SdlSetClipboardText(selectedText.TrimEnd('\0'));

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetClipboardText")]
        private static extern IntPtr SdlGetClipboardText();

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetClipboardText")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SdlSetClipboardText([MarshalAs(UnmanagedType.LPUTF8Str)] string text);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_free")]
        private static extern void SdlFree(IntPtr memory);
    }
}
