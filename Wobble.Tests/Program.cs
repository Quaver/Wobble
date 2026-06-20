using System;

using MonoGame.Framework.Utilities;

namespace Wobble.Tests
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var game = new WobbleTestsGame())
            {
                Console.WriteLine($"MonoGame graphics backend: {PlatformInfo.GraphicsBackend}");
                game.Run();
            }
        }
    }
}
