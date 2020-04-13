using System;
using Microsoft.Xna.Framework;

namespace Wobble.Window
{
    public class WindowVirtualScreenSizeChangedEventArgs : EventArgs
    {
        public Vector2 Size { get; }

        public WindowVirtualScreenSizeChangedEventArgs(Vector2 size) => Size = size;
    }
}