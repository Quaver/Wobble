using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.SpriteTextPlusFormattable
{
    public class TestSpriteTextPlusFormattableScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestSpriteTextPlusFormattableScreen() => View = new TestSpriteTextPlusFormattableScreenView(this);
    }
}