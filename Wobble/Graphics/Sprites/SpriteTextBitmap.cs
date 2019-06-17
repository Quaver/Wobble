using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Wobble.Assets;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    public class SpriteTextBitmap : Sprite
    {
        /// <summary>
        ///     The font used to draw the text
        /// </summary>
        public BitmapFont Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                DisplayedText = WrapText(Text);
                CachedTexture = false;
            }
        }

        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                DisplayedText = WrapText(value);
                CachedTexture = false;
            }
        }

        /// <summary>
        ///     The text that'll be displayed on-screen (after wrapping)
        /// </summary>
        public string DisplayedText { get; private set; }

        /// <summary>
        ///     The maximum width of the text.
        /// </summary>
        private int _maxWidth = int.MaxValue;
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                DisplayedText = WrapText(Text);
                CachedTexture = false;
            }
        }

        /// <summary>
        ///     Effects applied to the text when drawing it.
        /// </summary>
        public SpriteEffects Effects { get; set; }

        /// <summary>
        /// </summary>
        private bool CacheToRenderTarget { get; }

        /// <summary>
        /// </summary>
        private bool CachedTexture { get; set; }

        /// <summary>
        /// </summary>
        private RenderTarget2D RenderTarget { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="cacheToRenderTarget"></param>
        public SpriteTextBitmap(BitmapFont font, string text, bool cacheToRenderTarget = true)
        {
            CacheToRenderTarget = cacheToRenderTarget;
            Font = font;

            if (Font == null)
                return;

            FontSize = font.LineHeight;
            Text = text;
        }

        public override void Update(GameTime gameTime)
        {
            if (!CachedTexture && CacheToRenderTarget)
            {
                if (Image != null && Image != WobbleAssets.WhiteBox)
                    Image.Dispose();

                CacheTexture();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible || Font == null)
                return;

            if (CachedTexture && CacheToRenderTarget)
            {
                base.DrawToSpriteBatch();
                return;
            }
            else
            {
                GameBase.Game.SpriteBatch.DrawString(Font, DisplayedText, AbsolutePosition, _color, Rotation,
                    Vector2.Zero, new Vector2((float)FontSize / Font.LineHeight, (float)FontSize / Font.LineHeight),
                    Effects, 0, null);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (SpriteBatchOptions != null)
            {
                // If we actually have new SpriteBatchOptions to use,then
                // we want to end the previous SpriteBatch.
                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception)
                {
                    // ignored
                }

                GameBase.DefaultSpriteBatchInUse = false;

                // Begin the new SpriteBatch
                SpriteBatchOptions.Begin();

                DrawToSpriteBatch();
            }
            // If the default spritebatch isn't used, we'll want to use it here and draw the sprite.
            else if (!GameBase.DefaultSpriteBatchInUse && !UsePreviousSpriteBatchOptions)
            {
                try
                {
                    // End the previous SpriteBatch.
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception)
                {
                    // ignored
                }

                // Begin the default spriteBatch
                GameBase.DefaultSpriteBatchOptions.Begin();
                GameBase.DefaultSpriteBatchInUse = true;

                DrawToSpriteBatch();
            }
            // This must mean that the default SpriteBatch is in use, so we can just go ahead and draw the object.
            else
            {
                try
                {
                    DrawToSpriteBatch();
                }
                catch (Exception e)
                {
                    GameBase.DefaultSpriteBatchOptions.Begin();
                    GameBase.DefaultSpriteBatchInUse = true;

                    DrawToSpriteBatch();
                }
            }

            base.Draw(gameTime);
        }

        public override void Destroy()
        {
            if (CachedTexture)
            {
                Image?.Dispose();
                RenderTarget?.Dispose();
            }

            base.Destroy();
        }

        /// <summary>
        ///     Performs text wrapping based on
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string WrapText(string text)
        {
            if (Font == null)
                return null;

            if (Font.MeasureString(text).Width * ((float) FontSize / Font.LineHeight) < MaxWidth)
            {
                Width = Font.MeasureString(text).Width * ((float) FontSize / Font.LineHeight);
                Height = Font.MeasureString(text).Height * ((float)FontSize / Font.LineHeight);

                return text;
            }

            var words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = Font.MeasureString(" ").Width * ((float)FontSize / Font.LineHeight);

            for (var i = 0; i < words.Length; ++i)
            {
                Vector2 size = Font.MeasureString(words[i]) * ((float)FontSize / Font.LineHeight);

                if (linewidth + size.X < MaxWidth)
                    linewidth += size.X + spaceWidth;
                else
                {
                    if (i != 0)
                        wrappedText.Append("\n");

                    linewidth = size.X + spaceWidth;
                }
                wrappedText.Append(words[i]);
                wrappedText.Append(" ");
            }

            Width = Font.MeasureString(wrappedText.ToString()).Width * ((float)FontSize / Font.LineHeight);
            Height = Font.MeasureString(wrappedText.ToString()).Height * ((float)FontSize / Font.LineHeight);

            return wrappedText.ToString();
        }

        private void CacheTexture()
        {
            var (pixelWidth, pixelHeight) = AbsoluteSize;

            // ReSharper disable twice CompareOfFloatsByEqualityOperator
            if (pixelWidth == 0 || pixelHeight == 0 || string.IsNullOrEmpty(Text))
                return;

            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                if (pixelWidth < 1)
                    pixelWidth = 1;

                if (pixelHeight < 1)
                    pixelHeight = 1;

                if (RenderTarget != null)
                {
                    RenderTarget.Dispose();
                    Image.Dispose();

                    RenderTarget = null;
                    Image = null;
                }

                RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int)pixelWidth, (int)pixelHeight, false,
                    GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
                GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception e)
                {
                    // ignored

                }
                finally
                {
                    GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                }

                GameBase.Game.SpriteBatch.DrawString(Font, DisplayedText, new Vector2(0, 0), Color.White, Rotation,
                    Vector2.Zero, new Vector2((float)FontSize / Font.LineHeight, (float)FontSize / Font.LineHeight),
                    Effects, 0, null);

                GameBase.Game.SpriteBatch.End();

                Image = RenderTarget;

                GameBase.Game.GraphicsDevice.SetRenderTarget(null);
                CachedTexture = true;
            });
        }
    }
}
