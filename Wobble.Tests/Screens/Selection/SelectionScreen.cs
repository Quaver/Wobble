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
            {ScreenType.Audio, "Audio"}
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
        Audio
    }
}
