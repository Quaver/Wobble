using Wobble.Screens;

namespace Wobble.Tests.Screens.Selection
{
    public class SelectionScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public SelectionScreen() => View = new SelectionScreenView(this);

        public override void OnActivated() => TestNavigation.AttachTo(this);
    }
}
