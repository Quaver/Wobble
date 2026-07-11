using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.PersistentElements
{
    public sealed class TestPersistentElementsFirstScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestPersistentElementsFirstScreen() => View = new TestPersistentElementsScreenView(this);

        public override void OnActivated() =>
            ((TestPersistentElementsScreenView)View).Configure(TestPersistentElementsStage.First);
    }

    public sealed class TestPersistentElementsSecondScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestPersistentElementsSecondScreen() => View = new TestPersistentElementsScreenView(this);

        public override void OnActivated() =>
            ((TestPersistentElementsScreenView)View).Configure(TestPersistentElementsStage.Second);
    }
}
