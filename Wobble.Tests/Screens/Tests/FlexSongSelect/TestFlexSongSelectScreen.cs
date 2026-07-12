using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.FlexSongSelect
{
    public class TestFlexSongSelectScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestFlexSongSelectScreen() => View = new TestFlexSongSelectScreenView(this);
    }
}
