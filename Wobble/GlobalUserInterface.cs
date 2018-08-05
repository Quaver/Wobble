using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;

namespace Wobble
{
    public class GlobalUserInterface : Container
    {
        public Cursor Cursor { get; }

        public GlobalUserInterface() => Cursor = new Cursor(WobbleAssets.WhiteBox, 40) { Parent = this };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
