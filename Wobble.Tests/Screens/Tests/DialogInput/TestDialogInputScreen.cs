using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.DialogInput
{
    public class TestDialogInputScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestDialogInputScreen() => View = new TestDialogInputScreenView(this);
    }
}
