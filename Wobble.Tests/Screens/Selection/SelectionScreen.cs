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
            {ScreenType.DrawingSprites, "Screen_DrawingSprites"},
            {ScreenType.Rotation, "Screen_Rotation"},
            {ScreenType.DrawableScaling, "Screen_DrawableScaling"},
            {ScreenType.EasingAnimations, "Screen_EasingAnimations"},
            {ScreenType.Audio, "Screen_Audio"},
            {ScreenType.Discord, "Screen_Discord"},
            {ScreenType.Background, "Screen_Background"},
            {ScreenType.Scrolling, "Screen_Scrolling"},
            {ScreenType.BlurContainer, "Screen_BlurContainer"},
            {ScreenType.BlurredBackgroundImage, "Screen_BlurredBackgroundImage"},
            {ScreenType.TextInput, "Screen_TextInput"},
            {ScreenType.SpriteMaskContainer, "Screen_SpriteMaskContainer"},
            {ScreenType.SpriteAlphaMaskingBlend, "Screen_SpriteAlphaMaskingBlend"},
            {ScreenType.Primitives, "Screen_Primitives"},
            {ScreenType.ImGui, "Screen_ImGui"},
            {ScreenType.Scaling, "Screen_Scaling"},
            {ScreenType.TextSizes, "Screen_TextSizes"},
            {ScreenType.TaskHandler, "Screen_TaskHandler"},
            {ScreenType.SpriteTextPlus, "Screen_SpriteTextPlus"},
            {ScreenType.ScheduledUpdates, "Screen_ScheduledUpdates"},
            {ScreenType.Joystick, "Screen_Joystick"},
            {ScreenType.ButtonPerformance, "Screen_ButtonPerformance"},
            {ScreenType.NavigationBar, "Screen_NavigationBar"},
            {ScreenType.PersistentElements, "Screen_PersistentElements"},
            {ScreenType.Tooltips, "Screen_Tooltips"},
            {ScreenType.HorizontalClipping, "Screen_HorizontalClipping"},
            {ScreenType.MarqueeSpriteText, "Screen_MarqueeSpriteText"},
            {ScreenType.NineSliceSprite, "Screen_NineSliceSprite"}
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
        Rotation,
        DrawableScaling,
        EasingAnimations,
        Audio,
        Discord,
        Background,
        Scrolling,
        BlurContainer,
        BlurredBackgroundImage,
        TextInput,
        SpriteMaskContainer,
        SpriteAlphaMaskingBlend,
        Primitives,
        ImGui,
        Scaling,
        TextSizes,
        TaskHandler,
        SpriteTextPlus,
        ScheduledUpdates,
        Joystick,
        ButtonPerformance,
        NavigationBar,
        PersistentElements,
        Tooltips,
        HorizontalClipping,
        MarqueeSpriteText,
        NineSliceSprite
    }
}
