using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Wobble.Platform.Windows
{
    /// <summary>
    /// Owns an unnamed, auto-reset Windows timer for one-shot frame waits. No process policies are changed.
    /// </summary>
    internal sealed class WindowsFrameTimer : IDisposable
    {
        private const uint HighResolution = 0x2; // CREATE_WAITABLE_TIMER_HIGH_RESOLUTION
        private const uint RequiredAccess = 0x00100002; // SYNCHRONIZE | TIMER_MODIFY_STATE
        private readonly SafeWaitHandle handle;

        private WindowsFrameTimer(SafeWaitHandle handle) => this.handle = handle;

        /// <summary>
        /// Returns null when native waiting is unavailable. Tries high resolution before an ordinary timer.
        /// </summary>
        public static WindowsFrameTimer TryCreate()
        {
            if (!OperatingSystem.IsWindows())
                return null;

            try
            {
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    var candidate = CreateWaitableTimerExW(IntPtr.Zero, null,
                        attempt == 0 ? HighResolution : 0, RequiredAccess);
                    if (!candidate.IsInvalid)
                        return new WindowsFrameTimer(candidate);

                    candidate.Dispose();
                }
            }
            catch (Exception exception) when (exception is EntryPointNotFoundException || exception is DllNotFoundException)
            {
                // The caller can use its portable wait path instead.
            }

            return null;
        }

        /// <summary>
        /// Returns false if the timer cannot be armed or the wait fails, allowing the caller to retire it.
        /// </summary>
        public bool Wait(TimeSpan delay)
        {
            if (handle.IsClosed || handle.IsInvalid)
                return false;
            if (delay <= TimeSpan.Zero)
                return true;

            // Windows accepts relative delays as negative counts of 100 ns, the same unit as TimeSpan ticks.
            var relativeDueTime = -delay.Ticks;
            try
            {
                if (!SetWaitableTimerEx(handle, ref relativeDueTime, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0))
                    return false;

                // A broken timer must not block the game indefinitely. This timeout is not the pacing interval.
                var timeout = (uint)Math.Min(uint.MaxValue - 1d, Math.Ceiling(delay.TotalMilliseconds) + 1000d);
                return WaitForSingleObject(handle, timeout) == 0; // WAIT_OBJECT_0
            }
            catch (Exception exception) when (exception is EntryPointNotFoundException || exception is DllNotFoundException)
            {
                return false;
            }
        }

        public void Dispose() => handle.Dispose();

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeWaitHandle CreateWaitableTimerExW(IntPtr attributes, string name, uint flags, uint access);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWaitableTimerEx(SafeWaitHandle timer, ref long dueTime, int period,
            IntPtr callback, IntPtr callbackState, IntPtr wakeContext, uint tolerableDelay);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern uint WaitForSingleObject(SafeWaitHandle waitHandle, uint timeoutMilliseconds);
    }
}
