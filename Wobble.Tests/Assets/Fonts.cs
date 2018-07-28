using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;

namespace Wobble.Tests.Assets
{
    public static class Fonts
    {
        /// <summary>
        ///     aller_regular_16
        /// </summary>
        public static SpriteFont AllerRegular16 { get; private set; }

        /// <summary>
        ///     Loads all the fonts we have in the game.
        /// </summary>
        public static void Load()
        {
            AllerRegular16 = AssetLoader.LoadFont(ResourceStore.ResourceManager, "aller_regular_16");
        }
    }
}
