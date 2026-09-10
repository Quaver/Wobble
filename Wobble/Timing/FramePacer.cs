using System;
using System.Diagnostics;
using System.Threading;
using Wobble.Platform.Windows;

namespace Wobble.Timing
{
    /// <summary>
    /// Limits the game loop using evenly spaced frame slots on a monotonic clock.
    /// </summary>
    public sealed class FramePacer : IDisposable
    {
        private WindowsFrameTimer timer = WindowsFrameTimer.TryCreate();
        private long epoch;
        private long nextFrame;
        private bool disposed;

        public int FramesPerSecond { get; private set; }

        public void SetLimit(int framesPerSecond)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (framesPerSecond < 0)
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond));

            FramesPerSecond = framesPerSecond;
            Reset();
        }

        public void Reset()
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            epoch = Stopwatch.GetTimestamp();
            nextFrame = 1;
        }

        public void WaitForNextFrame()
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (FramesPerSecond == 0)
                return;

            // Calculate deadline from the epoch
            var deadline = epoch + (long)Math.Ceiling(nextFrame * (Stopwatch.Frequency / (double)FramesPerSecond));
            var now = Stopwatch.GetTimestamp();

            while (now < deadline)
            {
                var remaining = deadline - now;
                if (timer != null)
                {
                    var delayTicks = (long)Math.Ceiling(remaining * (TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency));
                    if (timer.Wait(TimeSpan.FromTicks(Math.Max(1, delayTicks))))
                    {
                        now = Stopwatch.GetTimestamp();
                        continue;
                    }

                    // Recalculate before the managed fallback
                    timer.Dispose();
                    timer = null;
                    now = Stopwatch.GetTimestamp();
                    remaining = deadline - now;
                }

                if (remaining > 0)
                {
                    var milliseconds = Math.Ceiling(remaining * 1000d / Stopwatch.Frequency);
                    Thread.Sleep((int)Math.Min(int.MaxValue, Math.Max(1, milliseconds)));
                }

                now = Stopwatch.GetTimestamp();
            }

            // Discard slots lost to slow frames or scheduling delays
            var elapsedSlots = (long)Math.Floor((now - epoch) * (FramesPerSecond / (double)Stopwatch.Frequency));
            nextFrame = Math.Max(nextFrame + 1, elapsedSlots + 1);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            timer?.Dispose();
            timer = null;
            disposed = true;
        }
    }
}
