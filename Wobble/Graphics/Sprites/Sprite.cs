using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

                Origin = new Vector2(Image.Width / 2f, Image.Height / 2f);
                RecalculateOrigin();
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        private float _rotation;
        public float Rotation
        {
            get => _rotation;
            set => _rotation = MathHelper.ToRadians(value);
        }

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
        public Rectangle RenderRectangle { get; private set; }

        /// <summary>
        ///    The rectangle for the origin of the sprite.
        /// </summary>
        public DrawRectangle OriginRectangle { get; } = new DrawRectangle();

        /// <summary>
        ///     The tint this QuaverSprite will inherit.
        /// </summary>
        private Color _tint = Color.White;
        private Color _color = Color.White;
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

                Children.ForEach(x =>
                {
                    var t = x.GetType();

                    if (t == typeof(Sprite))
                    {
                        var sprite = (Sprite) x;
                        sprite.Alpha = value;
                    }

                    // TODO: SpriteText
                });
            }
        }

        /// <summary>
        ///     Dictates if we want to set the alpha of the children as well.
        /// </summary>
        public bool SetChildrenAlpha { get; set; }

        /// <summary>
        ///     Constructor - Add event handler to recalculate origin.
        /// </summary>
        public Sprite() => RectangleRecalculated += (sender, args) => RecalculateOrigin();

        /// <inheritdoc />
        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Draw only if the image isn't null.
            if (Image != null)
                GameBase.Game.SpriteBatch.Draw(Image, RenderRectangle, null, _color, _rotation, Origin, SpriteEffect, 0f);

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        private void RecalculateOrigin()
        {
            if (Image == null)
                return;

            // Update Origin Rect
            OriginRectangle.Width = ScreenRectangle.Width;
            OriginRectangle.Height = ScreenRectangle.Height;
            OriginRectangle.X = ScreenRectangle.X + ScreenRectangle.Width / 2f;
            OriginRectangle.Y = ScreenRectangle.Y + ScreenRectangle.Height / 2f;

            // Update Render Rect
            RenderRectangle = new Rectangle((int)OriginRectangle.X, (int)OriginRectangle.Y,
                                                (int) OriginRectangle.Width,(int) OriginRectangle.Height);
        }
    }
}