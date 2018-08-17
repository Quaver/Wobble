using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TextInput
{
    public class TestTextInputScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestTextInputScreen() => View = new TestTextInputScreenView(this);
    }
}