using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wobble.Tests
{
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            using (var game = new WobbleTestsGame())
            {
                game.Run();
            }
        }
    }
}
