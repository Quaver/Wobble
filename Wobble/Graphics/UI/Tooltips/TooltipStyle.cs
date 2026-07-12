using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites.Text;

namespace Wobble.Graphics.UI.Tooltips
{
    public class TooltipStyle
    {
        public Color? BackgroundColor { get; set; }
        public Color? TextColor { get; set; }
        public int? TextSize { get; set; }
        public int? TextWeight { get; set; }
        public Color? BorderColor { get; set; }
        public float? BorderThickness { get; set; }
        public bool? RoundedCorners { get; set; }
        public float? CornerRadius { get; set; }
    }

    public class TooltipTheme
    {
        public Color BackgroundColor { get; set; } = new Color(30, 30, 34);
        public Color TextColor { get; set; } = Color.White;
        public int TextSize { get; set; } = 18;
        public int TextWeight { get; set; } = FontWeight.SemiBold;
        public Color BorderColor { get; set; } = new Color(90, 90, 98);
        public float BorderThickness { get; set; } = 1;
        public bool RoundedCorners { get; set; } = true;

        /// <summary>
        ///     The corner radius in pixels. A fixed default keeps multiline tooltips from becoming
        ///     increasingly pill-shaped as their wrapped content makes them taller. Set to null
        ///     explicitly for RoundedButton's full-pill radius.
        /// </summary>
        public float? CornerRadius { get; set; } = 8;
        public float Padding { get; set; } = 8;
        public float Offset { get; set; } = 8;
        public double HoverDelayMilliseconds { get; set; } = 350;
        public float MaximumWidth { get; set; } = 360;
        public IDictionary<int, WobbleFontStore> Fonts { get; set; } =
            new Dictionary<int, WobbleFontStore>();
    }
}
