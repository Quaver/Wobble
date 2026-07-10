using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.NavigationBars
{
    public class TestNavigationBarScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestNavigationBarScreen() => View = new TestNavigationBarScreenView(this);
    }
}
