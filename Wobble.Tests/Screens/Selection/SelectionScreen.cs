using System;
using System.Collections.Generic;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Selection
{
    public class SelectionScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The list of screens to be used for test cases
        /// </summary>
        public Dictionary<ScreenType, string> TestCasesScreens { get; } = new Dictionary<ScreenType, string>
        {
            {ScreenType.DrawingSprites, "Drawing Sprites"},
            {ScreenType.EasingAnimations, "Easing Animations"},
            {ScreenType.Audio, "Audio"},
            {ScreenType.Discord, "Discord Rich Pr."},
            {ScreenType.Background, "Background Sprite"},
            {ScreenType.Scrolling, "Scroll Container"},
            {ScreenType.BlurContainer, "Blur Container"},
            {ScreenType.BlurredBackgroundImage, "Blurred BG Image"},
            {ScreenType.TextInput, "Text Input"},
            {ScreenType.SpriteMaskContainer, "Sprite Masking"},
            {ScreenType.BitmapFont, "Bitmap Font"},
            {ScreenType.Primitives, "Primitives"},
            {ScreenType.ImGui, "Imgui"},
            {ScreenType.Scaling, "Scaling"},
            {ScreenType.TextSizes, "Text Sizes"},
            {ScreenType.TaskHandler, "Task Handler"},
            {ScreenType.SpriteTextPlus, "SpriteTextPlus"}
        };

        public SelectionScreen() => View = new SelectionScreenView(this);
    }

    /// <summary>
    ///     For every test screen, we give it an enum value so we can know which one to initialize.
    ///     See: SelectionScreenView.CreateSelectionButtons();
    /// </summary>
    public enum ScreenType
    {
        DrawingSprites,
        EasingAnimations,
        Audio,
        Discord,
        Background,
        Scrolling,
        BlurContainer,
        BlurredBackgroundImage,
        TextInput,
        SpriteMaskContainer,
        BitmapFont,
        Primitives,
        ImGui,
        Scaling,
        TextSizes,
        TaskHandler,
        SpriteTextPlus
    }
}
