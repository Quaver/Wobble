using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.FlexContainer
{
    public class TestFlexContainerScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestFlexContainerScreen() => View = new TestFlexContainerScreenView(this);
    }
}
