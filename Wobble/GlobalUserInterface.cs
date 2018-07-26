using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI;

namespace Wobble
{
    public class GlobalUserInterface : Container
    {
        public Cursor Cursor { get; }

        public GlobalUserInterface() => Cursor = new Cursor(WobbleAssets.WhiteBox, 40) { Parent = this };
    }
}
