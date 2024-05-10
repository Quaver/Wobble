using System;
using System.Threading;
using System.Threading.Tasks;
using Wobble.Logging;

namespace Wobble.Scheduling
{
    /// <summary>
    ///     This will be used to handle threaded tasks.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class TaskHandler<T, TResult> : IDisposable
    {
        /// <summary>
        /// This will be used for the task that the TaskHandler will be Handling.
        /// </summary>
        private Func<T, CancellationToken, TResult> Function { get; }

        /// <summary>
        /// This will prevent race conditions from happening when calling Run(), Cancel(), and Dispose()
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        ///     Event Invoked when the task gets completed.
        /// </summary>
        public event EventHandler<TaskCompleteEventArgs<T, TResult>> OnCompleted;

        /// <summary>
        ///     Event Invoked when the task gets cancelled.
        /// </summary>
        public event EventHandler<TaskCancelledEventArgs<T>> OnCancelled;

        /// <summary>
        ///     Used to handle Task Cancellation.
        /// </summary>
        public CancellationTokenSource Source { get; private set; } = new CancellationTokenSource();

        /// <summary>
        ///     Determined by if current task is completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        ///     Determined by if the current task is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Determined by if the current task is cancelled.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="function"></param>
        public TaskHandler(Func<T, CancellationToken, TResult> function) => Function = function;

        /// <summary>
        ///     This method will cancel the previous task and run a new task.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="delay"></param>
        public CancellationToken Run(T input, int delay = 0)
        {
            // We want to automatically cancel the previous task and dispose the other
            // cancellation token source before running a new task.
            lock (_lock)
            {
                Source.Cancel();
                Source.Dispose();
                Source = new CancellationTokenSource();

                IsCompleted = false;
                IsCancelled = false;
                IsRunning = true;
            }

            var token = Source.Token;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, token);

                    var result = Function(input, token);

                    lock (_lock)
                    {
                        token.ThrowIfCancellationRequested();
                        IsCompleted = true;
                        IsRunning = false;
                    }

                    // Invoke the callback outside of the lock to avoid accidental deadlocks
                    // caused by the callback code.
                    OnCompleted?.Invoke(typeof(TaskHandler<T, TResult>),
                        new TaskCompleteEventArgs<T, TResult>(input, result));
                }
                catch (OperationCanceledException)
                {
                    OnSourceCancelled(input);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            });

            return token;
        }

        /// <summary>
        ///     This method is called when a task is cancelled or if user wants to cancel existing task.
        /// </summary>
        /// <param name="input"></param>
        private void OnSourceCancelled(T input) => OnCancelled?.Invoke(typeof(TaskHandler<T, TResult>), new TaskCancelledEventArgs<T>(input));

        /// <summary>
        ///     Cancel the current existing task.
        /// </summary>
        public void Cancel()
        {
            lock (_lock)
            {
                Source.Cancel();
                IsCancelled = true;
                IsRunning = false;
            }
        }

        /// <summary>
        ///     Should be called when Task Handler will no longer be used.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                Source.Cancel();
                Source.Dispose();
                OnCompleted = null;
                OnCancelled = null;
            }
        }
    }
}
