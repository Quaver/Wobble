using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.HorizontalClipping
{
    public class TestHorizontalClippingScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestHorizontalClippingScreen() => View = new TestHorizontalClippingScreenView(this);
    }
}
