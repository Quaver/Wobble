using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Scrolling
{
    public class TestScrollContainerScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestScrollContainerScreen() => View = new TestScrollContainerScreenView(this);
    }
}