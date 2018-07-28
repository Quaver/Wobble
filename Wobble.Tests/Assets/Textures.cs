using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;

namespace Wobble.Tests.Assets
{
    public static class Textures
    {
        /// <summary>
        ///     test_spritesheet
        /// </summary>
        public static List<Texture2D> TestSpritesheet { get; private set; }

        /// <summary>
        ///     Loads all the textures for the game.
        /// </summary>
        public static void Load()
        {
            TestSpritesheet = AssetLoader.LoadSpritesheetFromTexture(AssetLoader.LoadTexture2D(ResourceStore.test_spritesheet, ImageFormat.Png), 1, 12);
        }
    }
}
