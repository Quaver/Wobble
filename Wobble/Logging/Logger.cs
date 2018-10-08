using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Wobble.Logging
{
    public static class Logger
    {
        /// <summary>
        ///     Dictates whether or not to display log messages (if in debug)
        /// </summary>
        public static bool DisplayMessages { get; set; } = true;

        /// <summary>
        ///     The folder which contains all the logs.
        /// </summary>
        public static string LogsFolder => $"{AppDomain.CurrentDomain.BaseDirectory}/Logs";

        /// <summary>
        ///     Gets the path of an individual LogType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetLogPath(LogType type) => $"{LogsFolder}/{type.ToString().ToLower()}.log";

        /// <summary>
        ///     Initializes the logger. Creates the folder and all necessary files.
        /// </summary>
        internal static void Initialize()
        {
            Directory.CreateDirectory(LogsFolder);

            foreach (LogType type in Enum.GetValues(typeof(LogType)))
            {
                if (File.Exists(GetLogPath(type)))
                    continue;

                using (var f = File.Create(GetLogPath(type)))
                {
                }
            }
        }

        /// <summary>
        ///     Standard debug log
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void Debug(string value, LogType type) => Log(value, LogLevel.Debug, type);

        /// <summary>
        ///     Logs an important message
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void Important(string value, LogType type) => Log(value, LogLevel.Important, type);

        /// <summary>
        ///     Logs a warning
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void Warning(string value, LogType type) => Log(value, LogLevel.Warning, type);

        /// <summary>
        ///     Logs an error
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void Error(string value, LogType type) => Log(value, LogLevel.Error, type);

        /// <summary>
        ///     Logs an error.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void Error(Exception value, LogType type) => Log(value.Message, LogLevel.Important, type);

        /// <summary>
        ///     Logs a message
        /// </summary>
        public static void Log(string m, LogLevel level, LogType type)
        {
            // Get a stringified version of the log level, and also set the color.
            var logLevelStr = "";
            switch (level)
            {
                case LogLevel.Debug:
                    logLevelStr = "DEBUG";
                    break;
                case LogLevel.Error:
                    logLevelStr = "ERROR";
                    break;
                case LogLevel.Important:
                    logLevelStr = "IMPORTANT";
                    break;
                case LogLevel.Warning:
                    logLevelStr = "WARNING";
                    break;
            }

            // Format the log
            var log = $"[{DateTime.Now:h:mm:ss}] - {type.ToString().ToUpper()} - {logLevelStr}: {m}";
            Console.WriteLine(log);

            // Write to the log file
            try
            {
                using (var sw = new StreamWriter(GetLogPath(type), true))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(log);
                }
            }
            catch (Exception e)
            {
                // If it fails, we can't really handle the error here. This shouldn't happen though.
                Console.WriteLine(e);
            }

#if DEBUG
            System.Diagnostics.Debug.Print(log);

            if (DisplayMessages)
                LogManager.AddLog(log, level);
#endif
        }
    }
}