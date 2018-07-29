using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Background
{
    public class TestBackgroundImageScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestBackgroundImageScreen() => View = new TestBackgroundImageScreenView(this);
    }
}
