using System;
using System.Collections.Generic;
using Wobble.Screens;

namespace Wobble.Extended.HotReload.Screens
{
    public class HotLoaderScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public HotLoaderScreen(Dictionary<string, Type> testScreens) => View = new HotLoaderScreenView(this, testScreens);
    }
}