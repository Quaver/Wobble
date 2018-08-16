using Wobble.Screens;
using Wobble.Tests.Screens.Tests.Background;

namespace Wobble.Tests.Screens.Tests.BlurredBgImage
{
    public class TestBlurredBackgroundImageScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestBlurredBackgroundImageScreen() => View = new TestBlurredBackgroundImageScreenView(this);
    }
}