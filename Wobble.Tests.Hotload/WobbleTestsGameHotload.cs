using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Extended.HotReload;
using Wobble.Extended.HotReload.Screens;
using Wobble.Graphics.Sprites.Text;
using Wobble.IO;
using Wobble.Managers;
using Wobble.Tests.Assets;
using Wobble.Tests.Screens.Tests.Audio;
using Wobble.Tests.Screens.Tests.Background;
using Wobble.Tests.Screens.Tests.BlurredBgImage;
using Wobble.Tests.Screens.Tests.Discord;
using Wobble.Tests.Screens.Tests.DrawingSprites;
using Wobble.Tests.Screens.Tests.EasingAnimations;
using Wobble.Tests.Screens.Tests.Imgui;
using Wobble.Tests.Screens.Tests.Primitives;
using Wobble.Tests.Screens.Tests.Scaling;
using Wobble.Tests.Screens.Tests.Scrolling;
using Wobble.Tests.Screens.Tests.SpriteMasking;
using Wobble.Tests.Screens.Tests.TextSizes;
using Wobble.Window;

namespace Wobble.Tests.Hotload
{
    public class WobbleTestsGameHotload : HotLoaderGame
    {
        public WobbleTestsGameHotload(HotLoader hl) : base(hl)
        {
            WindowManager.ChangeScreenResolution(new Point(1366, 768));
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Resources.AddStore(new DllResourceStore("Wobble.Tests.Resources.dll"));
            CacheFonts();

            base.LoadContent();

            IsReadyToUpdate = true;
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        protected override HotLoaderScreen InitializeHotLoaderScreen() => new HotLoaderScreen(new Dictionary<string, Type>()
        {
            {"Drawing Sprites", typeof(TestDrawingSpritesScreen)},
            {"Easing", typeof(TestEasingAnimationsScreen)},
            {"ImGUI", typeof(TestImGuiScreen)},
            {"Audio", typeof(TestAudioScreen)},
            {"Background", typeof(TestBackgroundImageScreen)},
            {"Blurred BG Image", typeof(TestBlurredBackgroundImageScreen)},
            {"Discord", typeof(TestDiscordScreen)},
            {"Primitives", typeof(TestPrimitivesScreen)},
            {"Scaling", typeof(TestScalingScreen)},
            {"Scrolling", typeof(TestScrollContainerScreen)},
            {"Sprite Masking", typeof(TestSpriteMaskingScreen)},
            {"TextSizes", typeof(TestTextSizesScreen)}
        });

        private static void CacheFonts()
        {
            var fonts = new List<string> { "exo2-bold", "exo2-regular", "exo2-semibold", "exo2-medium" };
            foreach (var fontName in fonts)
            {
                if (FontManager.WobbleFonts.ContainsKey(fontName))
                    continue;

                FontManager.CacheWobbleFont(fontName,
                    new WobbleFontStore(20, GameBase.Game.Resources.Get($"Wobble.Tests.Resources/Fonts/{fontName}.ttf")));
            }
        }
    }
}
