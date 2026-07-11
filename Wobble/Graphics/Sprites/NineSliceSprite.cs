using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Sprites
{
    /// <summary>
    ///     Defines the margins used to divide a texture into nine slices.
    /// </summary>
    public struct SliceMargins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public SliceMargins(int all) : this(all, all, all, all)
        {
        }

        public SliceMargins(int horizontal, int vertical) : this(horizontal, horizontal, vertical, vertical)
        {
        }

        public SliceMargins(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }

    /// <summary>
    ///     Draws a texture using nine-slice scaling. Corners retain their size, edges stretch on one axis,
    ///     and the center stretches on both axes.
    /// </summary>
    public class NineSliceSprite : Sprite
    {
        private SliceMargins _margins;

        private Rectangle _srcTopLeft, _srcTopCenter, _srcTopRight;
        private Rectangle _srcMiddleLeft, _srcMiddleCenter, _srcMiddleRight;
        private Rectangle _srcBottomLeft, _srcBottomCenter, _srcBottomRight;

        public override Texture2D Image
        {
            get => base.Image;
            set
            {
                base.Image = value;
                RecalculateSourceRectangles();
            }
        }

        /// <summary>
        ///     The margins defining how <see cref="Image"/> is divided.
        /// </summary>
        public SliceMargins Margins
        {
            get => _margins;
            set
            {
                _margins = value;
                RecalculateSourceRectangles();
            }
        }

        public NineSliceSprite()
        {
        }

        public NineSliceSprite(Texture2D texture, SliceMargins margins)
        {
            _margins = margins;
            Image = texture;
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            var width = Math.Max(0, (int)ScreenRectangle.Width);
            var height = Math.Max(0, (int)ScreenRectangle.Height);
            var leftWidth = Math.Min(Math.Max(0, _margins.Left), width);
            var rightWidth = Math.Min(Math.Max(0, _margins.Right), width - leftWidth);
            var centerWidth = width - leftWidth - rightWidth;
            var topHeight = Math.Min(Math.Max(0, _margins.Top), height);
            var bottomHeight = Math.Min(Math.Max(0, _margins.Bottom), height - topHeight);
            var centerHeight = height - topHeight - bottomHeight;

            var x0 = (int)ScreenRectangle.X;
            var x1 = x0 + leftWidth;
            var x2 = x1 + centerWidth;
            var y0 = (int)ScreenRectangle.Y;
            var y1 = y0 + topHeight;
            var y2 = y1 + centerHeight;
            var spriteBatch = GameBase.Game.SpriteBatch;

            DrawSlice(spriteBatch, new Rectangle(x0, y0, leftWidth, topHeight), _srcTopLeft);
            DrawSlice(spriteBatch, new Rectangle(x1, y0, centerWidth, topHeight), _srcTopCenter);
            DrawSlice(spriteBatch, new Rectangle(x2, y0, rightWidth, topHeight), _srcTopRight);
            DrawSlice(spriteBatch, new Rectangle(x0, y1, leftWidth, centerHeight), _srcMiddleLeft);
            DrawSlice(spriteBatch, new Rectangle(x1, y1, centerWidth, centerHeight), _srcMiddleCenter);
            DrawSlice(spriteBatch, new Rectangle(x2, y1, rightWidth, centerHeight), _srcMiddleRight);
            DrawSlice(spriteBatch, new Rectangle(x0, y2, leftWidth, bottomHeight), _srcBottomLeft);
            DrawSlice(spriteBatch, new Rectangle(x1, y2, centerWidth, bottomHeight), _srcBottomCenter);
            DrawSlice(spriteBatch, new Rectangle(x2, y2, rightWidth, bottomHeight), _srcBottomRight);
        }

        private void DrawSlice(SpriteBatch spriteBatch, Rectangle destination, Rectangle source)
        {
            if (destination.Width > 0 && destination.Height > 0 && source.Width > 0 && source.Height > 0)
                spriteBatch.Draw(Image, destination, source, _color);
        }

        private void RecalculateSourceRectangles()
        {
            if (Image == null || Image.IsDisposed)
                return;

            var left = Math.Min(Math.Max(0, _margins.Left), Image.Width);
            var right = Math.Min(Math.Max(0, _margins.Right), Image.Width - left);
            var centerWidth = Image.Width - left - right;
            var top = Math.Min(Math.Max(0, _margins.Top), Image.Height);
            var bottom = Math.Min(Math.Max(0, _margins.Bottom), Image.Height - top);
            var centerHeight = Image.Height - top - bottom;

            _srcTopLeft = new Rectangle(0, 0, left, top);
            _srcTopCenter = new Rectangle(left, 0, centerWidth, top);
            _srcTopRight = new Rectangle(left + centerWidth, 0, right, top);
            _srcMiddleLeft = new Rectangle(0, top, left, centerHeight);
            _srcMiddleCenter = new Rectangle(left, top, centerWidth, centerHeight);
            _srcMiddleRight = new Rectangle(left + centerWidth, top, right, centerHeight);
            _srcBottomLeft = new Rectangle(0, top + centerHeight, left, bottom);
            _srcBottomCenter = new Rectangle(left, top + centerHeight, centerWidth, bottom);
            _srcBottomRight = new Rectangle(left + centerWidth, top + centerHeight, right, bottom);
        }
    }
}
