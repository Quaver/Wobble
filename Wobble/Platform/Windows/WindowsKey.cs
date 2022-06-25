using System;
using System.Runtime.InteropServices;

namespace Wobble.Platform.Windows
{
    /// <summary>
    ///     https://social.msdn.microsoft.com/Forums/vstudio/en-US/3440fc53-30a4-4ef7-84a2-e875f26a1bf9/keyboard-hook-not-working?forum=csharpgeneral
    /// </summary>
    public static class WindowsKey
    {
        /// <summary>
        ///     If the key is currently disabled
        /// </summary>
        private static bool IsDisabled { get; set; }

        [DllImport(@"user32.dll", EntryPoint = @"SetWindowsHookExA", CharSet = CharSet.Ansi)]
        private static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport(@"user32.dll")]
        private static extern int UnhookWindowsHookEx(int hHook);

        [DllImport(@"user32.dll", EntryPoint = @"CallNextHookEx", CharSet = CharSet.Ansi)]
        private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        private const int WH_KEYBOARD_LL = 13;

        private static int Hook { get; set; }

        private static KBDLLHOOKSTRUCT Lparam;

        private static LowLevelKeyboardProcDelegate lowLevelKeyboardProcDelegate;

        /// <summary>
        ///     Enables the windows key
        /// </summary>
        internal static void EnableWindowsKey()
        {
            if (Hook == 0 || !IsDisabled)
                return;

            try
            {
                Hook = UnhookWindowsHookEx(Hook);
                lowLevelKeyboardProcDelegate = null;
            }
            catch (Exception)
            {
                // ignored
            }

            IsDisabled = false;
            Hook = 0;
        }

        /// <summary>
        ///     Disable Windows Key
        /// </summary>
        internal static void DisableWindowsKey()
        {
            if (Hook != 0 || IsDisabled)
                return;

            try
            {
                Hook = SetWindowsHookEx(WH_KEYBOARD_LL, lowLevelKeyboardProcDelegate = LowLevelKeyboardProc,
                    Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);
            }
            catch (Exception)
            {
                // ignored
            }

            IsDisabled = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            switch (wParam)
            {
                case 256:
                case 257:
                case 260:
                case 261:
                    if ((lParam.VkCode == 91 || lParam.VkCode == 92) && lParam.Flags == 1)
                        return 1;
                    break;
            }

            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }
    }

    public struct KBDLLHOOKSTRUCT
    {
        public int VkCode { get; set; }
        private int ScanCode { get; set; }
        public int Flags { get; set; }
        private int Time { get; set; }
        private int DwExtraInfo { get; set; }
    }
}
