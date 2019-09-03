using System;

namespace Wobble.Scheduling
{
    public class TaskStartedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     Input given to task before starting.
        /// </summary>
        public T Input { get; }

        public TaskStartedEventArgs(T input) => Input = input;
    }
}