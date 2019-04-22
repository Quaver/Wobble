using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Scaling
{
    public class TestScalingScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        public TestScalingScreen() => View = new TestScalingScreenView(this);
    }
}
