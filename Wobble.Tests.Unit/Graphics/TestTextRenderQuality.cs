using Wobble.Graphics.Sprites.Text;
using Xunit;

namespace Wobble.Tests.Unit.Graphics
{
    public class TestTextRenderQuality
    {
        [Theory]
        [InlineData(10.24f, 2f, 10f)]
        [InlineData(10.26f, 2f, 10.5f)]
        [InlineData(-10.26f, 2f, -10.5f)]
        [InlineData(3.2f, 0f, 3f)]
        public void SnapRoundsToNearestPhysicalPixel(float position, float scale, float expected)
        {
            Assert.Equal(expected, TextRenderQuality.Snap(position, scale), 4);
        }
    }
}
