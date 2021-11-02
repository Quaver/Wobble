using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Joystick
{
    public class TestJoystickScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestJoystickScreen() => View = new TestJoystickScreenView(this);
    }
}