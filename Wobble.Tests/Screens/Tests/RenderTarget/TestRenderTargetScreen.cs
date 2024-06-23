using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.RenderTarget
{
    public class TestRenderTargetScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public TestRenderTargetScreen() => View = new TestRenderTargetScreenView(this);
    }
}