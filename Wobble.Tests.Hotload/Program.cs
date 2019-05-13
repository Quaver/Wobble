using System;
using System.IO;
using Wobble.Extended.HotReload;

namespace Wobble.Tests.Hotload
{
    internal static class Program
    {
        /// <summary>
        ///     The current working directory of the executable.
        /// </summary>
        public static string WorkingDirectory => WobbleGame.WorkingDirectory;

        [STAThread]
        internal static void Main(string[] args)
        {
            // Change the working directory to where the executable is.
            Directory.SetCurrentDirectory(WorkingDirectory);
            Environment.CurrentDirectory = WorkingDirectory;
            
            using (var game = new WobbleTestsGameHotload(new HotLoader("../../../../Wobble.Tests/")))
                game.Run();
        }
    }
}