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
using Wobble.Tests.Screens.Tests.Tooltips;
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
            Resources.AddStore(new DllResourceStore("Wobble.Tests.Resources.dll"));
            CacheFonts();

            base.Initialize();

            Window.AllowUserResizing = true;
        }

        protected override void LoadContent()
        {
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
            {"TextSizes", typeof(TestTextSizesScreen)},
            {"Tooltips", typeof(TestTooltipsScreen)}
        });

        private static void CacheFonts()
        {
            if (FontManager.WobbleFonts.ContainsKey("inter-regular"))
                return;

            var interFont = GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/Inter/Inter.ttf");
            var emojiFont = GameBase.Game.Resources.Get("Wobble.Tests.Resources/Fonts/NotoColorEmoji/NotoColorEmoji.ttf");

            CacheInterFont("inter-regular", FontWeight.Regular, interFont, emojiFont);
            CacheInterFont("inter-medium", FontWeight.Medium, interFont, emojiFont);
            CacheInterFont("inter-semibold", FontWeight.SemiBold, interFont, emojiFont);
            CacheInterFont("inter-bold", FontWeight.Bold, interFont, emojiFont);
        }

        private static void CacheInterFont(string name, int weight, byte[] interFont, byte[] emojiFont)
        {
            FontManager.CacheWobbleFont(name, new WobbleFontStore(20,
                new WobbleFontFace(interFont, weight: weight),
                new Dictionary<string, WobbleFontFace>()
                {
                    {"Emoji", new WobbleFontFace(emojiFont)}
                }));
        }
    }
}
