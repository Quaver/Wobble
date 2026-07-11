using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Tooltips
{
    public class TestTooltipsScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestTooltipsScreen() => View = new TestTooltipsScreenView(this);
    }
}
