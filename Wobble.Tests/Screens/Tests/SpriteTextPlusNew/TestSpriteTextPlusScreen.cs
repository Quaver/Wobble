using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.SpriteTextPlusNew
{
    public class TestSpriteTextPlusScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestSpriteTextPlusScreen() => View = new TestSpriteTextPlusScreenView(this);
    }
}