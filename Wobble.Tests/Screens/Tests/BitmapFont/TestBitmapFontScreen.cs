using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.BitmapFont
{
    public class TestBitmapFontScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public TestBitmapFontScreen() => View = new TestBitmapFontScreenView(this);
    }
}