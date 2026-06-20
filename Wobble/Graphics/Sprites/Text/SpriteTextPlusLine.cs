using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Logging;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLine : Sprite
    {
        /// <summary>
        ///     The underlying text rendering component.
        /// </summary>
        private SpriteTextPlusLineRaw _raw;

        /// <summary>
        ///     Whether the cached texture needs to be refreshed.
        /// </summary>
        private bool _dirty;

        /// <summary>
        ///     Current WindowManager scale.
        /// </summary>
        private float _scale;
        private Vector2 _cacheScale;
        private Vector2 CacheScale
        {
            get => _cacheScale;
            set
            {
                if (_cacheScale == value)
                    return;

                // Retrieve the original font size (computed with old scale).
                var fontSize = FontSize;

                _cacheScale = value;
                _scale = value.Y;
                _raw.Scale = new Vector2(value.X / value.Y, 1f);

                // Set the font size with the new scale.
                FontSize = fontSize;
            }
        }

        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get => _raw.Font; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        public float FontSize
        {
            get => _raw.FontSize / _scale;
            set
            {
                _raw.FontSize = value * _scale;
                SetSize();
                _dirty = true;
            }
        }

        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        public string Text
        {
            get => _raw.Text;
            set
            {
                _raw.Text = value;
                SetSize();
                _dirty = true;
            }
        }

        /// <summary>
        ///     The rendertarget used to cache the text
        /// </summary>
        private RenderTarget2D RenderTarget { get; set; }

        /// <summary>
        ///     Whether this line owns and draws a render target. SpriteTextPlus disables this and
        ///     caches all of its lines into one render target instead.
        /// </summary>
        internal bool CacheEnabled { get; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLine(WobbleFontStore font, string text, float size = 0)
            : this(font, text, size, true)
        {
        }

        internal SpriteTextPlusLine(WobbleFontStore font, string text, float size, bool cacheEnabled)
        {
            CacheEnabled = cacheEnabled;
            _cacheScale = TextRenderQuality.CacheScale;
            _scale = _cacheScale.Y;

            _raw = new SpriteTextPlusLineRaw(font, text, size * _scale)
            {
                Scale = new Vector2(_cacheScale.X / _cacheScale.Y, 1f),
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    DoNotScale = true,
                    BlendState = BlendState.AlphaBlend
                }
            };

            SetSize();

            Image = WobbleAssets.WhiteBox;
            _dirty = true;
        }

        /// <summary>
        ///     Get the current WindowManager scale and check that it's valid.
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetScale() => TextRenderQuality.CacheScale;

        /// <summary>
        ///     Set the component size taking rounding into account.
        /// </summary>
        private void SetSize()
        {
            // Round the size the same way it will be rounded during rendering.
            var (width, height) = _raw.AbsoluteSize;
            var pixelWidth = Math.Ceiling(width);
            var pixelHeight = Math.Ceiling(height);

            Size = new ScalableVector2(
                (float)pixelWidth / _cacheScale.X,
                (float)pixelHeight / _cacheScale.Y);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update the Scale and schedules the component to be rendered into a texture if necessary.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            CacheScale = GetScale();

            if (_dirty && CacheEnabled)
            {
                _dirty = false;
                GameBase.Game.ScheduledRenderTargetDraws.Add(() => Cache(gameTime));
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (RenderTarget != null && !RenderTarget.IsDisposed)
                RenderTarget.Dispose();

            Image = null;

            base.Destroy();
        }

        public override void DrawToSpriteBatch()
        {
            if (!CacheEnabled)
                return;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusDraw(true);
#endif

            base.DrawToSpriteBatch();
        }

        /// <summary>
        ///     Round the position to align with pixels exactly.
        /// </summary>
        protected override void OnRectangleRecalculated()
        {
            // Update the render rectangle.
            var x = ScreenRectangle.X;
            var y = ScreenRectangle.Y;

            if (Rotation == 0)
            {
                // Round the coordinates. Not rounding the coordinates means bad text.
                var displayScale = TextRenderQuality.DisplayScale;
                x = TextRenderQuality.Snap(x, displayScale.X);
                y = TextRenderQuality.Snap(y, displayScale.Y);

                // Keep both edges on physical pixels. This avoids filtering caused by a
                // fractionally-sized destination rectangle even when its origin is snapped.
                var right = TextRenderQuality.Snap(x + ScreenRectangle.Width, displayScale.X);
                var bottom = TextRenderQuality.Snap(y + ScreenRectangle.Height, displayScale.Y);
                var snappedWidth = Math.Max(0, right - x);
                var snappedHeight = Math.Max(0, bottom - y);

                RenderRectangle = new RectangleF(x + snappedWidth / 2f, y + snappedHeight / 2f,
                    snappedWidth, snappedHeight);
                return;
            }

            // Add Width / 2 and Height / 2 to X, Y because that's what Origin is set to (in the Image setter).
            RenderRectangle = new RectangleF(x + ScreenRectangle.Width / 2f, y + ScreenRectangle.Height / 2f,
                ScreenRectangle.Width, ScreenRectangle.Height);
        }

        /// <summary>
        ///     Render the text into a texture.
        /// </summary>
        /// <param name="gameTime"></param>
        private void Cache(GameTime gameTime)
        {
            if (IsDisposed)
                return;

#if DEBUG
            global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordSpriteTextPlusCacheBuild();
#endif

            _ = GameBase.Game.TryEndBatch();
            var (width, height) = _raw.AbsoluteSize;
            var pixelWidth = (int) Math.Ceiling(width);
            var pixelHeight = (int) Math.Ceiling(height);

            if (pixelWidth == 0 || pixelHeight == 0)
            {
                Visible = false;
                return;
            }

            Visible = true;

            if (RenderTarget == null || RenderTarget.IsDisposed ||
                RenderTarget.Width != pixelWidth || RenderTarget.Height != pixelHeight)
            {
                if (RenderTarget != null && !RenderTarget.IsDisposed)
                    RenderTarget.Dispose();

                RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, pixelWidth, pixelHeight, false,
                    GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            }

            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);
            _raw.Draw(gameTime);
            _ = GameBase.Game.TryEndBatch();

            GameBase.Game.GraphicsDevice.SetRenderTarget(null);

            Image = RenderTarget;
        }
    }
}
