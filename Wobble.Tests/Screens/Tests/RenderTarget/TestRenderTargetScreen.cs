using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.RenderTarget
{
    public class TestRenderTargetScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestRenderTargetScreen() => View = new TestRenderTargetScreenView(this);
    }
}
