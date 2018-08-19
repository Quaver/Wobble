using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.SpriteMasking
{
    public class TestSpriteMaskingScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestSpriteMaskingScreen() => View = new TestSpriteMaskingScreenView(this);
    }
}