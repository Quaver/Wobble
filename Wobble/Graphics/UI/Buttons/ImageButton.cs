using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.UI.Buttons
{
    public class ImageButton : Button
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="clickAction"></param>
        public ImageButton(Texture2D image, EventHandler clickAction = null) : base(clickAction) => Image = image;
    }
}
