using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TextSizes
{
    public class TestTextSizesScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        public TestTextSizesScreen() => View = new TestTextSizesScreenView(this);
    }
}
