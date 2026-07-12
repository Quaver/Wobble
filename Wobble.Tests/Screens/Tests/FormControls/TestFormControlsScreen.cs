using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.FormControls
{
    public class TestFormControlsScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestFormControlsScreen() => View = new TestFormControlsScreenView(this);
    }
}
