using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TextLine
{
    public class TestSpriteTextPlusLineScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public TestSpriteTextPlusLineScreen() => View = new TestSpriteTextPlusLineScreenView(this);
    }
}
