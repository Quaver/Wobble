using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.GlobalUiScale
{
    public class TestGlobalUiScaleScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestGlobalUiScaleScreen() => View = new TestGlobalUiScaleScreenView(this);
    }
}
