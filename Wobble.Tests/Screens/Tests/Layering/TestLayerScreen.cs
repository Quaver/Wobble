using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Layering
{
    public class TestLayerScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestLayerScreen() => View = new TestLayerScreenView(this);
    }
}