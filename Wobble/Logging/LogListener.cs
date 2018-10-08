using System.Collections.Generic;
using System.Diagnostics;

namespace Wobble.Logging
{
    public class LogListener : TraceListener
    {
        public List<string> Logs { get; } = new List<string>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            lock (Logs)
                Logs.Add(message);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            lock (Logs)
                Logs.Add(message);
        }

        public List<string> GetLogs()
        {
            lock (Logs)
            {
                var logs = new List<string>(Logs);
                Logs.Clear();
                return logs;
            }
        }
    }
}