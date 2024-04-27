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
        private float Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                    return;

                // Retrieve the original font size (computed with old scale).
                var fontSize = FontSize;

                _scale = value;

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
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLine(WobbleFontStore font, string text, float size = 0)
        {
            _scale = GetScale();

            _raw = new SpriteTextPlusLineRaw(font, text, size * _scale)
            {
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
        private static float GetScale()
        {
            var scale = WindowManager.ScreenScale.X;
            Debug.Assert(scale > 0, "You're setting up text too early (WindowManager.ScreenScale.X is 0).");

            if (GameBase.Game.Graphics.PreferredBackBufferWidth < 1600)
                return scale * 2;

            return scale;
        }

        /// <summary>
        ///     Set the component size taking rounding into account.
        /// </summary>
        private void SetSize()
        {
            // Round the size the same way it will be rounded during rendering.
            var (width, height) = _raw.AbsoluteSize;
            var pixelWidth = Math.Ceiling(width);
            var pixelHeight = Math.Ceiling(height);

            var flooredSize = new ScalableVector2((float) pixelWidth, (float) pixelHeight);
            Size = flooredSize / _scale;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update the Scale and schedules the component to be rendered into a texture if necessary.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Scale = GetScale();

            if (_dirty)
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
                var pixelX = (int) (x * WindowManager.ScreenScale.X);
                var pixelY = (int) (y * WindowManager.ScreenScale.Y);

                x = pixelX / WindowManager.ScreenScale.X;
                y = pixelY / WindowManager.ScreenScale.Y;
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

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored
            }

            var (width, height) = _raw.AbsoluteSize;
            var pixelWidth = (int) Math.Ceiling(width);
            var pixelHeight = (int) Math.Ceiling(height);

            if (pixelWidth == 0 || pixelHeight == 0)
            {
                Visible = false;
                return;
            }

            Visible = true;

            if (RenderTarget != null && !RenderTarget.IsDisposed)
                RenderTarget?.Dispose();

            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, pixelWidth, pixelHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);
            _raw.Draw(gameTime);
            GameBase.Game.SpriteBatch.End();

            GameBase.Game.GraphicsDevice.SetRenderTarget(null);

            Image = RenderTarget;
        }
    }
}