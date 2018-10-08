using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Primitives
{
    public class TestPrimitivesScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        public TestPrimitivesScreen() => View = new TestPrimitivesScreenView(this);
    }
}
