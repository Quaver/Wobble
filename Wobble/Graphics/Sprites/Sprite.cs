using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Window;

namespace Wobble.Graphics.Sprites
{
    public class Sprite : Drawable
    {
        /// <summary>
        ///     the image texture of the sprite.
        /// </summary>
        private Texture2D _image;
        public Texture2D Image
        {
            get => _image;
            set
            {
                if (value == null)
                    return;

                _image = value;

                Origin = new Vector2(Image.Width * Pivot.X, Image.Height * Pivot.Y);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The XNA SpriteEffects the sprite will have.
        ///     When the sprite is negatively scaled on some axis, it will be flipped over that axis too.
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
        public float Alpha
        {
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

        /// <summary>
        ///     Dictates if we want to set the alpha of the children as well.
        /// </summary>
        public bool SetChildrenAlpha { get; set; }

        /// <summary>
        ///     Additional rotation applied to this sprite only, and not to its children
        /// </summary>
        public float SpriteRotation
        {
            get => _spriteRotation;
            set
            {
                _spriteRotation = value;
                SpriteOverallRotation = _spriteRotation + (IndependentRotation ? Rotation : AbsoluteRotation);
            }
        }

        private bool _independentRotation;
        private float _spriteRotation;

        /// <summary>
        ///     If true, the rotation of sprite shown on screen will be independent of its parent.
        /// </summary>
        public bool IndependentRotation
        {
            get => _independentRotation;
            set
            {
                _independentRotation = value;
                SpriteRotation = SpriteRotation;
            }
        }

        /// <summary>
        ///     Actual rotation of sprite shown on screen.
        ///     It is decided by <see cref="IndependentRotation"/> and parent's <see cref="Drawable.AbsoluteRotation"/>
        /// </summary>
        public float SpriteOverallRotation { get; protected set; }

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            GameBase.Game.SpriteBatch.Draw(Image, RenderRectangle, null, _color, SpriteOverallRotation, Origin, SpriteEffect, 0f);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SpriteBatchOptions?.Shader?.Dispose();
            base.Destroy();
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        protected override void OnRectangleRecalculated()
        {
            if (Image == null)
                return;

            var pivot = Pivot;
            var screenRectangleSize = ScreenRectangle.Size;

            Origin = new Vector2(pivot.X * Image.Width, pivot.Y * Image.Height);

            // The render rectangle's position will rotate around the screen rectangle's position
            var rotatedScreenOrigin = (ScreenRectangle.Size * Pivot).Rotate(Parent?.AbsoluteRotation ?? 0);

            // Update the render rectangle
            RenderRectangle = new RectangleF(
                ScreenRectangle.Position + rotatedScreenOrigin,
                screenRectangleSize);

            SpriteRotation = SpriteRotation;
        }

        /// <summary>
        ///     Fades the sprite to a given color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        public virtual void FadeToColor(Color color, double dt, float scale)
        {
            var r = MathHelper.Lerp(Tint.R, color.R, (float)Math.Min(dt / scale, 1));
            var g = MathHelper.Lerp(Tint.G, color.G, (float)Math.Min(dt / scale, 1));
            var b = MathHelper.Lerp(Tint.B, color.B, (float)Math.Min(dt / scale, 1));

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
