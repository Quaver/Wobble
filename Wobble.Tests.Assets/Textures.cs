using System.Collections.Generic;
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
        ///     left_button_square
        /// </summary>
        public static Texture2D LeftButtonSquare { get; private set; }

        /// <summary>
        ///     right_button_square
        /// </summary>
        public static Texture2D RightButtonSquare { get; private set; }

        /// <summary>
        ///     circle-mask
        /// </summary>
        public static Texture2D CircleMask { get; private set; }

        /// <summary>
        ///     Loads all the textures for the game.
        /// </summary>
        public static void Load()
        {
            var testSpritesheetTex = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Textures/test_spritesheet.png"));
            TestSpritesheet = AssetLoader.LoadSpritesheetFromTexture(testSpritesheetTex, 1, 12);

            LeftButtonSquare = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Textures/left_button_square.png"));
            RightButtonSquare = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Textures/right_button_square.png"));
            CircleMask = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Textures/circle-mask.png"));
        }
    }
}
