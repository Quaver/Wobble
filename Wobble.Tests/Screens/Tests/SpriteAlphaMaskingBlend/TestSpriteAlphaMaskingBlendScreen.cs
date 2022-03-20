using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.SpriteAlphaMaskingBlend
{
    public class TestSpriteAlphaMaskingBlendScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestSpriteAlphaMaskingBlendScreen() => View = new TestSpriteAlphaMaskingBlendScreenView(this);
    }
}
