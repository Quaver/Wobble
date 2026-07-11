using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.NineSliceSprite
{
    public class TestNineSliceSpriteScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestNineSliceSpriteScreen() => View = new TestNineSliceSpriteScreenView(this);
    }
}
