using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.DrawingSprites
{
    public class TestDrawingSpritesScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestDrawingSpritesScreen() => View = new TestDrawingSpritesScreenView(this);
    }
}
