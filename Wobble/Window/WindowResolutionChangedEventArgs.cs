using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Wobble.Window
{
    public class WindowResolutionChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     The new resolution that the window was changed to
        /// </summary>
        public Point NewResolution { get; }

        /// <summary>
        ///     The old resolution that the window was.
        /// </summary>
        public Point OldResolution { get; }


        public WindowResolutionChangedEventArgs(Point newResolution, Point oldResolution)
        {
            NewResolution = newResolution;
            OldResolution = oldResolution;
        }
    }
}
