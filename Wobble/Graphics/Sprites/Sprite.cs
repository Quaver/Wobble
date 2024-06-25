using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics.Animations;

namespace Wobble.Graphics.Sprites
{
    public class Sprite : Drawable
    {
        /// <summary>
        ///     the image texture of the sprite which is drawn on screen
        /// </summary>
        private Texture2D _image;

        /// <summary>
        ///     the source texture of the sprite. If there are passes to the shaders, _image will be changed
        ///     but this won't
        /// </summary>
        private Texture2D _originalTexture;

        /// <summary>
        ///     If <see cref="AdditionalPasses"/> is not empty, this is used for applying shaders in them
        /// </summary>
        private RenderTarget2D _intermediateImage;

        private Container _boundProjectionContainerSource;

        public Texture2D Image
        {
            get => _image;
            set
            {
                if (value == null)
                    return;

                _image = _originalTexture = value;

                Origin = new Vector2(Image.Width * Pivot.X, Image.Height * Pivot.Y);

                _intermediateImage = new RenderTarget2D(GameBase.Game.GraphicsDevice, _image.Width, _image.Height, false,
                    GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                if (AdditionalPasses != null && AdditionalPasses.Count > 0) 
                    GameBase.Game.ScheduledRenderTargetDraws.Add(PerformAdditionalPasses);

                RecalculateRectangles();
            }
        }
        public List<SpriteBatchOptions> AdditionalPasses { get; set; }

        /// <summary>
        ///     The XNA SpriteEffects the sprite will have.
        /// </summary>
        public SpriteEffects SpriteEffect { get; set; } = SpriteEffects.None;

        /// <summary>
        ///     The origin of this object used for rotation.
        /// </summary>
        public Vector2 Origin { get; private set; }

        /// <summary>
        ///     The rectangle used to render the sprite.
        /// </summary>
        public RectangleF RenderRectangle { get; set; }

        /// <summary>
        ///     The tint this QuaverSprite will inherit.
        /// </summary>
        private Color _tint = Color.White;
        public Color _color = Color.White;
        public Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            }
        }

        /// <summary>
        ///     The transparency of this QuaverSprite.
        /// </summary>
        private float _alpha = 1f;
        public float Alpha {
            get => _alpha;
            set
            {
                _alpha = value;
                _color = _tint * _alpha;

                if (!SetChildrenAlpha)
                    return;

                for (var i = 0; i < Children.Count; i++)
                {
                    var x = Children[i];

                    if (x is Sprite sprite)
                    {
                        sprite.Alpha = value;
                    }
                }
            }
        }

        private bool _independentRotation;

        /// <summary>
        ///     If true, the rotation of sprite shown on screen will be independent of its parent.
        /// </summary>
        public bool IndependentRotation
        {
            get => _independentRotation;
            set
            {
                _independentRotation = value;
                SpriteRotation = value ? Rotation : AbsoluteRotation;
            }
        }

        /// <summary>
        ///     Actual rotation of sprite shown on screen.
        ///     It is decided by <see cref="IndependentRotation"/> and parent's <see cref="Drawable.AbsoluteRotation"/>
        /// </summary>
        public float SpriteRotation { get; private set; }

        /// <summary>
        ///     Dictates if we want to set the alpha of the children as well.
        /// </summary>
        public bool SetChildrenAlpha { get; set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_originalTexture is RenderTarget2D && AdditionalPasses != null && AdditionalPasses.Count > 0) 
                GameBase.Game.ScheduledRenderTargetDraws.Add(PerformAdditionalPasses);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // If there is no image set, create a dummy 1px one.
            if (Image == null)
                Image = WobbleAssets.WhiteBox;

            if (SpriteBatchOptions != null)
            {
                // If we actually have new SpriteBatchOptions to use,then
                // we want to end the previous SpriteBatch.
                _ = GameBase.Game.TryEndBatch();

                GameBase.DefaultSpriteBatchInUse = false;

                // Begin the new SpriteBatch
                SpriteBatchOptions.Begin();

                DrawToSpriteBatch();
            }
            // If the default spritebatch isn't used, we'll want to use it here and draw the sprite.
            else if (!GameBase.DefaultSpriteBatchInUse && !UsePreviousSpriteBatchOptions)
            {
                _ = GameBase.Game.TryEndBatch();

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
                catch (Exception)
                {
                    GameBase.DefaultSpriteBatchOptions.Begin();
                    GameBase.DefaultSpriteBatchInUse = true;

                    DrawToSpriteBatch();
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Transforms the <see cref="_originalTexture"/> from <see cref="AdditionalPasses"/>
        ///     Due to the nature of the MonoGame's drawing order,
        ///     Changes to <see cref="Image"/> will be delayed by one frame
        /// </summary>
        private void PerformAdditionalPasses(GameTime gameTime)
        {
            _ = GameBase.Game.TryEndBatch();
            GameBase.Game.GraphicsDevice.SetRenderTarget(_intermediateImage);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

            for (var index = 0; index < AdditionalPasses.Count; index++)
            {
                var pass = AdditionalPasses[index];
                pass.Begin();

                var target = index == 0 ? _originalTexture : _intermediateImage;

                GameBase.Game.SpriteBatch.Draw(target, new RectangleF(Point2.Zero, new Size2(target.Width, target.Height)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
            }

            _ = GameBase.Game.TryEndBatch();
            GameBase.Game.GraphicsDevice.SetRenderTarget(null);
            _image = _intermediateImage;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            GameBase.Game.SpriteBatch.Draw(Image, RenderRectangle, null, _color, SpriteRotation, Origin, SpriteEffect, 0f);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SpriteBatchOptions?.Shader?.Dispose();
            if (_boundProjectionContainerSource != null)
                _boundProjectionContainerSource.RenderTarget.ValueChanged -= OnRenderTargetChange;
            base.Destroy();
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        protected override void OnRectangleRecalculated()
        {
            if (Image == null)
                return;

            Origin = Pivot * Image.Bounds.Size.ToVector2();

            // The render rectangle's position will rotate around the screen rectangle's position
            var rotatedScreenOrigin = (ScreenRectangle.Size * Pivot).Rotate(Parent?.AbsoluteRotation ?? 0);

            // Update the render rectangle
            RenderRectangle = new RectangleF(
                ScreenRectangle.Position + rotatedScreenOrigin,
                ScreenRectangle.Size);

            SpriteRotation = IndependentRotation ? Rotation : AbsoluteRotation;
        }

        /// <summary>
        ///     When called, the sprite will show the image of the container instead.
        ///     If the container is not drawing to render target, it will automatically do so
        /// </summary>
        /// <param name="container">The container to project its drawing from</param>
        public void BindProjectionContainer(Container container)
        {
            if (_boundProjectionContainerSource != null)
                _boundProjectionContainerSource.RenderTarget.ValueChanged -= OnRenderTargetChange;

            _boundProjectionContainerSource = container;

            if (_boundProjectionContainerSource.RenderTarget?.Value == null)
                _boundProjectionContainerSource.CastToRenderTarget();

            Image = container.RenderTarget.Value;
            container.RenderTarget.ValueChanged += OnRenderTargetChange;
        }

        private void OnRenderTargetChange(object sender, BindableValueChangedEventArgs<RenderTarget2D> target2D)
        {
            Image = target2D.Value;
        }

        /// <summary>
        ///     Fades the sprite to a given color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        public virtual void FadeToColor(Color color, double dt, float scale)
        {
            var r = MathHelper.Lerp(Tint.R, color.R, (float) Math.Min(dt / scale, 1));
            var g = MathHelper.Lerp(Tint.G, color.G, (float) Math.Min(dt / scale, 1));
            var b = MathHelper.Lerp(Tint.B, color.B, (float) Math.Min(dt / scale, 1));

            Tint = new Color((int)r, (int)g, (int)b);
        }

        /// <summary>
        ///     Fades to a tint in a certain amount of time.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        public Sprite FadeToColor(Color color, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(easingType, Tint, color, time));

            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Sprite FadeTo(float alpha, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.Alpha, easingType, Alpha, alpha, time));

            return this;
        }
    }
}
