using System;
using Microsoft.Xna.Framework;

namespace ExampleGame
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var game = new ExampleGame())
            {
                game.Run();
            }
        }
    }
}