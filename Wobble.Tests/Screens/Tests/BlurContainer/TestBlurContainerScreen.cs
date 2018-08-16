using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.BlurContainer
{
    public class TestBlurContainerScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public TestBlurContainerScreen() => View = new TestBlurContainerScreenView(this);
    }
}