using System.Collections.Generic;
using Wobble.Configuration;
using Wobble.Graphics.UI.Navigation;

namespace Wobble.Tests.Screens.Tests.NavigationBars
{
    public sealed class NavigationBarTestConfiguration
    {
        [ConfigEditable]
        public NavigationBarBackgroundType BackgroundType { get; set; } = NavigationBarBackgroundType.SolidColor;

        [ConfigEditable]
        public NavigationBarTestColor SolidColor { get; set; } = new NavigationBarTestColor(28, 49, 68);

        [ConfigEditable]
        public NavigationBarTestImageConfiguration Image { get; set; } = new NavigationBarTestImageConfiguration();

        [ConfigEditable]
        public NavigationBarTestGradientConfiguration Gradient { get; set; } =
            new NavigationBarTestGradientConfiguration();
    }

    public sealed class NavigationBarTestImageConfiguration
    {
        public string Asset { get; set; } = "Wallpaper";

        public NavigationBarImageFit Fit { get; set; } = NavigationBarImageFit.Stretch;
    }

    public sealed class NavigationBarTestGradientConfiguration
    {
        public NavigationBarGradientType Type { get; set; } = NavigationBarGradientType.Linear;

        public List<NavigationBarTestGradientStop> Stops { get; set; } = new List<NavigationBarTestGradientStop>
        {
            new NavigationBarTestGradientStop(0, new NavigationBarTestColor(11, 132, 255)),
            new NavigationBarTestGradientStop(0.45m, new NavigationBarTestColor(111, 66, 193)),
            new NavigationBarTestGradientStop(1, new NavigationBarTestColor(238, 74, 137))
        };

        public decimal AngleDegrees { get; set; }

        public NavigationBarTestPoint RadialOrigin { get; set; } = new NavigationBarTestPoint(0.5m, 0.5m);

        public decimal RadialRadius { get; set; } = 1;
    }

    public sealed class NavigationBarTestGradientStop
    {
        public decimal Position { get; set; }

        public NavigationBarTestColor Color { get; set; } = new NavigationBarTestColor();

        public NavigationBarTestGradientStop()
        {
        }

        public NavigationBarTestGradientStop(decimal position, NavigationBarTestColor color)
        {
            Position = position;
            Color = color;
        }
    }

    public sealed class NavigationBarTestColor
    {
        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public byte A { get; set; } = byte.MaxValue;

        public NavigationBarTestColor()
        {
        }

        public NavigationBarTestColor(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    public sealed class NavigationBarTestPoint
    {
        public decimal X { get; set; }

        public decimal Y { get; set; }

        public NavigationBarTestPoint()
        {
        }

        public NavigationBarTestPoint(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
    }
}
