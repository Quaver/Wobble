using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TextVerticalLayout
{
    public class TestTextVerticalLayoutScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestTextVerticalLayoutScreen() => View = new TestTextVerticalLayoutScreenView(this);
    }
}
