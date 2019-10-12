using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.ScheduledUpdates
{
    public sealed class TestScheduledUpdatesScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScheduledUpdatesScreen() => View = new TestScheduledUpdatesScreenView(this);
    }
}