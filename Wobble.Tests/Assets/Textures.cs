using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Managers;

namespace Wobble.Tests.Assets
{
    public static class Textures
    {
        public static Texture2D CircleMask => TextureManager.Load("Wobble.Tests.Resources/Textures/circle-mask.png");

        public static Texture2D LeftButtonSquare => TextureManager.Load($"Wobble.Tests.Resources/Textures/left_button_square.png");

        public static Texture2D RightButtonSquare => TextureManager.Load($"Wobble.Tests.Resources/Textures/right_button_square.png");

        public static List<Texture2D> TestSpriteSheet => TextureManager.LoadAtlas($"Wobble.Tests.Resources/Textures/test_spritesheet.png", 1, 12);
    }
}