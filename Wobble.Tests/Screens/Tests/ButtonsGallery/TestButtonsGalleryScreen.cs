using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.ButtonsGallery
{
    public class TestButtonsGalleryScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestButtonsGalleryScreen() => View = new TestButtonsGalleryScreenView(this);
    }
}
