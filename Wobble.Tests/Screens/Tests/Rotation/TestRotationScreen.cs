using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Rotation
{
    public class TestRotationScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestRotationScreen() => View = new TestRotationScreenView(this);
    }
}
