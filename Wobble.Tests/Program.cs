using System;

namespace Wobble.Tests
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var game = new WobbleTestsGame())
                game.Run();
        }
    }
}
