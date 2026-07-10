using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.ButtonPerformance
{
    public class TestButtonPerformanceScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestButtonPerformanceScreen() => View = new TestButtonPerformanceScreenView(this);
    }
}
