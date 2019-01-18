using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Imgui
{
    public class TestImGuiScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestImGuiScreen() => View = new TestImGuiScreenView(this);
    }
}