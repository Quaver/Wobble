using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.AnimationTiming
{
    public class TestAnimationTimingScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestAnimationTimingScreen() => View = new TestAnimationTimingScreenView(this);

        public override void OnActivated() => ((TestAnimationTimingScreenView) View).Activate();
    }
}
