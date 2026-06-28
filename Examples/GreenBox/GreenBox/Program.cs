using System;

namespace GreenBox
{
    internal class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            using (var game = new MyGame())
                game.Run();
        }
    }
}
