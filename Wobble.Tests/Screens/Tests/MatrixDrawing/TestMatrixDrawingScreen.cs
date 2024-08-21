using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.MatrixDrawing
{
    public class TestMatrixDrawingScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestMatrixDrawingScreen() => View = new TestMatrixDrawingScreenView(this);
    }
}
