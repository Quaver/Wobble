using System;
using System.Collections.Generic;
using Wobble.Screens;
using Wobble.Tests.Screens.Tests.Audio;
using Wobble.Tests.Screens.Tests.Background;
using Wobble.Tests.Screens.Tests.BlurContainer;
using Wobble.Tests.Screens.Tests.BlurredBgImage;
using Wobble.Tests.Screens.Tests.ButtonPerformance;
using Wobble.Tests.Screens.Tests.CursorScaling;
using Wobble.Tests.Screens.Tests.Discord;
using Wobble.Tests.Screens.Tests.DialogInput;
using Wobble.Tests.Screens.Tests.DrawableScaling;
using Wobble.Tests.Screens.Tests.DrawingSprites;
using Wobble.Tests.Screens.Tests.EasingAnimations;
using Wobble.Tests.Screens.Tests.HorizontalClipping;
using Wobble.Tests.Screens.Tests.Imgui;
using Wobble.Tests.Screens.Tests.Joystick;
using Wobble.Tests.Screens.Tests.MarqueeSpriteText;
using Wobble.Tests.Screens.Tests.NavigationBars;
using Wobble.Tests.Screens.Tests.NineSliceSprite;
using Wobble.Tests.Screens.Tests.PersistentElements;
using Wobble.Tests.Screens.Tests.Primitives;
using Wobble.Tests.Screens.Tests.Rotation;
using Wobble.Tests.Screens.Tests.Scaling;
using Wobble.Tests.Screens.Tests.ScheduledUpdates;
using Wobble.Tests.Screens.Tests.Scrolling;
using Wobble.Tests.Screens.Tests.SpriteAlphaMaskingBlend;
using Wobble.Tests.Screens.Tests.SpriteMasking;
using Wobble.Tests.Screens.Tests.SpriteTextPlusNew;
using Wobble.Tests.Screens.Tests.TaskHandler;
using Wobble.Tests.Screens.Tests.TextInput;
using Wobble.Tests.Screens.Tests.TextSizes;
using Wobble.Tests.Screens.Tests.Tooltips;

namespace Wobble.Tests.Screens.Selection
{
    internal sealed class TestScreenDescriptor
    {
        public string CategoryKey { get; }
        public string LabelKey { get; }
        public Func<Screen> CreateScreen { get; }
        public bool Isolated { get; }

        public TestScreenDescriptor(string categoryKey, string labelKey, Func<Screen> createScreen,
            bool isolated = false)
        {
            CategoryKey = categoryKey;
            LabelKey = labelKey;
            CreateScreen = createScreen;
            Isolated = isolated;
        }
    }

    internal static class TestScreenRegistry
    {
        private const string Rendering = "Category_Rendering";
        private const string LayoutMotion = "Category_LayoutMotion";
        private const string TextControls = "Category_TextControls";
        private const string InputIntegration = "Category_InputIntegration";
        private const string RuntimeFramework = "Category_RuntimeFramework";

        public static readonly IReadOnlyList<string> Categories = new[]
        {
            Rendering, LayoutMotion, TextControls, InputIntegration, RuntimeFramework
        };

        public static readonly IReadOnlyList<TestScreenDescriptor> Screens = new[]
        {
            new TestScreenDescriptor(Rendering, "Screen_DrawingSprites", () => new TestDrawingSpritesScreen()),
            new TestScreenDescriptor(Rendering, "Screen_Background", () => new TestBackgroundImageScreen()),
            new TestScreenDescriptor(Rendering, "Screen_BlurContainer", () => new TestBlurContainerScreen()),
            new TestScreenDescriptor(Rendering, "Screen_BlurredBackgroundImage", () => new TestBlurredBackgroundImageScreen()),
            new TestScreenDescriptor(Rendering, "Screen_SpriteMaskContainer", () => new TestSpriteMaskingScreen()),
            new TestScreenDescriptor(Rendering, "Screen_SpriteAlphaMaskingBlend", () => new TestSpriteAlphaMaskingBlendScreen()),
            new TestScreenDescriptor(Rendering, "Screen_Primitives", () => new TestPrimitivesScreen()),
            new TestScreenDescriptor(Rendering, "Screen_NineSliceSprite", () => new TestNineSliceSpriteScreen()),
            new TestScreenDescriptor(Rendering, "Screen_HorizontalClipping", () => new TestHorizontalClippingScreen()),

            new TestScreenDescriptor(LayoutMotion, "Screen_Rotation", () => new TestRotationScreen()),
            new TestScreenDescriptor(LayoutMotion, "Screen_DrawableScaling", () => new TestDrawableScalingScreen()),
            new TestScreenDescriptor(LayoutMotion, "Screen_Scaling", () => new TestScalingScreen()),
            new TestScreenDescriptor(LayoutMotion, "Screen_EasingAnimations", () => new TestEasingAnimationsScreen()),
            new TestScreenDescriptor(LayoutMotion, "Screen_Scrolling", () => new TestScrollContainerScreen()),
            new TestScreenDescriptor(LayoutMotion, "Screen_MarqueeSpriteText", () => new TestMarqueeSpriteTextScreen()),

            new TestScreenDescriptor(TextControls, "Screen_TextSizes", () => new TestTextSizesScreen()),
            new TestScreenDescriptor(TextControls, "Screen_SpriteTextPlus", () => new TestSpriteTextPlusScreen()),
            new TestScreenDescriptor(TextControls, "Screen_TextInput", () => new TestTextInputScreen()),
            new TestScreenDescriptor(TextControls, "Screen_Tooltips", () => new TestTooltipsScreen()),
            new TestScreenDescriptor(TextControls, "Screen_ButtonPerformance", () => new TestButtonPerformanceScreen()),

            new TestScreenDescriptor(InputIntegration, "Screen_Audio", () => new TestAudioScreen()),
            new TestScreenDescriptor(InputIntegration, "Screen_CursorScaling", () => new TestCursorScalingScreen()),
            new TestScreenDescriptor(InputIntegration, "Screen_Joystick", () => new TestJoystickScreen()),
            new TestScreenDescriptor(InputIntegration, "Screen_Discord", () => new TestDiscordScreen()),
            new TestScreenDescriptor(InputIntegration, "Screen_DialogInput", () => new TestDialogInputScreen()),
            new TestScreenDescriptor(InputIntegration, "Screen_ImGui", () => new TestImGuiScreen()),

            new TestScreenDescriptor(RuntimeFramework, "Screen_TaskHandler", () => new TaskHandlerScreen()),
            new TestScreenDescriptor(RuntimeFramework, "Screen_ScheduledUpdates", () => new TestScheduledUpdatesScreen()),
            new TestScreenDescriptor(RuntimeFramework, "Screen_NavigationBar", () => new TestNavigationBarScreen(), true),
            new TestScreenDescriptor(RuntimeFramework, "Screen_PersistentElements", () => new TestPersistentElementsFirstScreen(), true)
        };
    }
}
