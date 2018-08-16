using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Blur
{
    public class TestBlurScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public TestBlurScreen() => View = new TestBlurScreenView(this);
    }
}