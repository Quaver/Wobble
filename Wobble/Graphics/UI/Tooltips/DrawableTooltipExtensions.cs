using System;

namespace Wobble.Graphics.UI.Tooltips
{
    public static class DrawableTooltipExtensions
    {
        public static IDisposable AddTooltip(this Drawable drawable, string text) =>
            TooltipManager.Attach(drawable, new TooltipOptions(text));

        public static IDisposable AddTooltip(this Drawable drawable, TooltipOptions options) =>
            TooltipManager.Attach(drawable, options);
    }
}
