using Wobble.Screens;
using Wobble.Tests.Screens.Tests.Rotation;

namespace Wobble.Tests.Screens.Tests.DrawableScaling
{
    public class TestDrawableScalingScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestDrawableScalingScreen() => View = new TestDrawableScalingScreenView(this);
    }
}
