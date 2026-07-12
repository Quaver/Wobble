using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.CursorScaling
{
    public class TestCursorScalingScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestCursorScalingScreen() => View = new TestCursorScalingScreenView(this);
    }
}
