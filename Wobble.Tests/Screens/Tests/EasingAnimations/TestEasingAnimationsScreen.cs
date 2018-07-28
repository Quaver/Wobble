using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.EasingAnimations
{
    public class TestEasingAnimationsScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestEasingAnimationsScreen() => View = new TestEasingAnimationsScreenView(this);
    }
}
