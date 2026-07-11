using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.MarqueeSpriteText
{
    public class TestMarqueeSpriteTextScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestMarqueeSpriteTextScreen() => View = new TestMarqueeSpriteTextScreenView(this);
    }
}
