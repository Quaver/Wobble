using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.IO;
using Wobble.Tests.Resources.SpriteFonts;

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
            AllerRegular16 = AssetLoader.LoadFont(WobbleTestsResourceFonts.ResourceManager, "aller_regular_16");
        }
    }
}
