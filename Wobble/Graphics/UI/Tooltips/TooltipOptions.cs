namespace Wobble.Graphics.UI.Tooltips
{
    public class TooltipOptions
    {
        public string Text { get; set; } = "";
        public TooltipAnchor Anchor { get; set; } = TooltipAnchor.TopCenter;
        public float? MaximumWidth { get; set; }
        public float? Offset { get; set; }
        public double? HoverDelayMilliseconds { get; set; }
        public float? Padding { get; set; }
        public TooltipStyle Style { get; set; }

        public TooltipOptions()
        {
        }

        public TooltipOptions(string text) => Text = text;
    }
}
