using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     A single-line text drawable that scrolls horizontally when it exceeds its available width.
    /// </summary>
    public class MarqueeSpriteText : HorizontalClippingContainer
    {
        private const float ScrollSpeed = 0.05f;
        private const double EndpointWaitTime = 2000;
        private const float VerticalPadding = 10;

        private double _timer;
        private MarqueeState _state = MarqueeState.WaitingStart;
        private float _scrollX;

        public SpriteTextPlus TextSprite { get; }

        public bool IsActive { get; set; }

        /// <summary>
        ///     The time the marquee waits before it begins scrolling.
        /// </summary>
        public double StartDelayMilliseconds { get; set; } = EndpointWaitTime;

        public MarqueeSpriteText(WobbleFontStore font, string text, int fontSize, float width)
        {
            TextSprite = new SpriteTextPlus(font, text, fontSize)
            {
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };

            Size = new ScalableVector2(width, TextSprite.Height + VerticalPadding);
            TextSprite.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!IsActive || TextSprite.Width <= Width)
            {
                Reset();
                return;
            }

            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
            var maxScroll = TextSprite.Width - Width;

            switch (_state)
            {
                case MarqueeState.WaitingStart:
                    _timer += elapsedMilliseconds;
                    if (_timer >= StartDelayMilliseconds)
                    {
                        _timer = 0;
                        _state = MarqueeState.ScrollingLeft;
                    }
                    break;

                case MarqueeState.ScrollingLeft:
                    _scrollX += (float)(ScrollSpeed * elapsedMilliseconds);
                    if (_scrollX >= maxScroll)
                    {
                        _scrollX = maxScroll;
                        _state = MarqueeState.WaitingEnd;
                    }
                    TextSprite.X = -_scrollX;
                    break;

                case MarqueeState.WaitingEnd:
                    _timer += elapsedMilliseconds;
                    if (_timer >= EndpointWaitTime)
                    {
                        _timer = 0;
                        _state = MarqueeState.ScrollingRight;
                    }
                    break;

                case MarqueeState.ScrollingRight:
                    _scrollX -= (float)(ScrollSpeed * 2 * elapsedMilliseconds);
                    if (_scrollX <= 0)
                    {
                        _scrollX = 0;
                        _state = MarqueeState.WaitingStart;
                    }
                    TextSprite.X = -_scrollX;
                    break;
            }
        }

        private void Reset()
        {
            _scrollX = 0;
            _timer = 0;
            _state = MarqueeState.WaitingStart;
            TextSprite.X = 0;
        }

        private enum MarqueeState
        {
            WaitingStart,
            ScrollingLeft,
            WaitingEnd,
            ScrollingRight
        }
    }
}
