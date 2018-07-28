using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OpenGL;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Window;

namespace Wobble.Graphics
{
    /// <summary>
    ///     Any objects that are drawn to the screen should derive from this.
    /// </summary>
    public abstract class Drawable : IDrawable, IDisposable
    {
        /// <summary>
        ///     The total amount of drawables that are drawn on-screen.
        /// </summary>
        public static int TotalDrawn { get; private set; }

        /// <summary>
        ///     The order of which this object was drawn. Higher means the object was drawn later.
        /// </summary>
        public int DrawOrder { get; private set; }

        /// <summary>
        ///     The parent of this drawable in which it depends on for its position and size.
        /// </summary>
        private Drawable _parent;
        public Drawable Parent
        {
            get => _parent;
            set
            {
                // If this drawable previously had a parent, remove it from the old parent's list
                // of children.
                _parent?.Children.Remove(this);

                // If we do end up having a non-null value for the new parent, we'll want to
                // add this drawable to their list of children.
                if (value != null)
                {
                    value.Children.Add(this);
                }
                else
                {
                    // If we've received null for the parent however, that must mean we want to FULLY
                    // destroy and dispose of the object.
                    for (var i = Children.Count - 1; i >= 0 ; i--)
                        Children[i].Destroy();
                }

                _parent = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The children of this drawable. All children objects depend on this object's position
        ///     and size.
        /// </summary>
        public List<Drawable> Children { get; } = new List<Drawable>();

        /// <summary>
        ///     The drawable's rectangle relative to the entire screen.
        /// </summary>
        public DrawRectangle ScreenRectangle { get; private set; } = new DrawRectangle();

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        public DrawRectangle RelativeRectangle { get; private set; } = new DrawRectangle();

        /// <summary>
        ///     The position of the drawable
        /// </summary>
        private ScalableVector2 _position = new ScalableVector2(0, 0);
        public ScalableVector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The size of the drawable.
        /// </summary>
        private ScalableVector2 _size = new ScalableVector2(0, 0);
        public ScalableVector2 Size
        {
            get => _size;
            set
            {
                var width = MathHelper.Clamp(value.X.Value, 0, int.MaxValue);
                var height = MathHelper.Clamp(value.Y.Value, 0, int.MaxValue);

                _size = new ScalableVector2(width, height, value.X.Scale, value.Y.Scale);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The X position of the object.
        /// </summary>
        public float X
        {
            get => Position.X.Value;
            set
            {
                Position = new ScalableVector2(value, Position.Y.Value, Position.X.Scale, Position.Y.Scale);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The Y position of the object.
        /// </summary>
        public float Y
        {
            get => Position.Y.Value;
            set
            {
                Position = new ScalableVector2(Position.X.Value, value, Position.X.Scale, Position.Y.Scale);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The width of the object.
        /// </summary>
        public float Width
        {
            get => Size.X.Value;
            set
            {
                value = MathHelper.Clamp(value, 0, int.MaxValue);

                Size = new ScalableVector2(value, Size.Y.Value, Size.X.Scale, Size.Y.Scale);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The width scale of the object.
        /// </summary>
        public float WidthScale
        {
            get => Size.X.Scale;
            set
            {
                Size = new ScalableVector2(Size.X.Value, Size.Y.Value, value, Size.Y.Scale);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The height of the object.
        /// </summary>
        public float Height
        {
            get => Size.Y.Value;
            set
            {
                value = MathHelper.Clamp(value, 0, int.MaxValue);

                Size = new ScalableVector2(Size.X.Value, value, Size.X.Scale, Size.Y.Scale);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The height scale of the object.
        /// </summary>
        public float HeightScale
        {
            get => Size.Y.Scale;
            set
            {
                Size = new ScalableVector2(Size.X.Value, Size.Y.Value, Size.X.Scale, value);
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The alignment of the object.
        /// </summary>
        private Alignment _alignment = Alignment.TopLeft;
        public Alignment Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                RecalculateRectangles();
            }
        }

        /// <summary>
        ///     The absolute size of the object relative to the screen.
        /// </summary>
        public Vector2 AbsoluteSize => new Vector2(ScreenRectangle.Width, ScreenRectangle.Height);

        /// <summary>
        ///     The absolute position of the object relative to the screen.
        /// </summary>
        public Vector2 AbsolutePosition => new Vector2(ScreenRectangle.X, ScreenRectangle.Y);

        /// <summary>
        ///     Determines if the object is going to get drawn.
        /// </summary>
        private bool _visible = true;
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;

                // Set children visibility if specified.
                if (SetChildrenVisibility)
                    Children.ForEach(x => x.Visible = value);
            }
        }

        /// <summary>
        ///     Dictates whether or not when we set the visibility of this object, the children's
        ///     get set as well.
        /// </summary>
        public bool SetChildrenVisibility { get; set; }

        /// <summary>
        ///     If the drawable has been disposed of already.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     The options that'll be used on SpriteBatch.Begin();
        ///     for this particular drawable. If left null, it'll use the default parameters and won't
        ///     create a new SpriteBatch.
        ///
        ///     This should only be set if you want extra functionality such as shaders or different
        ///     blend states for example.
        /// </summary>
        public SpriteBatchOptions SpriteBatchOptions { get; set; }

        /// <summary>
        ///     If set, it'll ignore calling SpriteBatch.End(); and creating a new SpriteBatch
        ///     This comes in handy if you want to have multiple sprites under the same
        ///     SpriteBatch.
        ///
        ///     This should normally only be set for actual drawable objects and non-containers;
        ///     things that actually get drawn to the SpriteBatch.
        /// </summary>
        public bool UsePreviousSpriteBatchOptions { get; set; }

        /// <summary>
        ///     The list of transformations to perform on this drawable.
        /// </summary>
        public List<Transformation> Transformations { get; } = new List<Transformation>();

        /// <summary>
        ///     Event raised when the rectangle has been recalculated.
        /// </summary>
        protected event EventHandler RectangleRecalculated;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            PerformTransformations(gameTime);

            // Update all of the contained children.
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                try
                {
                    Children[i].Update(gameTime);
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            // Increase the total amount of drawables that were drawn and set the order to the current
            // total.
            TotalDrawn++;
            DrawOrder = TotalDrawn;

            // Draw the children and set their order.
            foreach (var drawable in Children)
            {
                drawable.Draw(gameTime);

                TotalDrawn++;
                drawable.DrawOrder = TotalDrawn;
            }
        }

        /// <summary>
        ///     Destroys the object. Removes the parent object. Any derivates should
        ///     free any resources used by the object.
        /// </summary>
        public virtual void Destroy()
        {
            Dispose();
            Parent = null;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Any derivatives should implement this disposition methods and then call
        ///     this base method to signify that it actually has been disposed of.
        /// </summary>
        public virtual void Dispose()
        {
            RectangleRecalculated = null;
            IsDisposed = true;
        }

        /// <summary>
        ///     Recalculates the local and global rectangles of the object. Makes sure that the position
        ///     and sizes are relative to the parent if the drawable has one.
        /// </summary>
        protected void RecalculateRectangles()
        {
            // Make it relative to the parent.
            if (Parent != null)
            {
                var width = Size.X.Value + Parent.ScreenRectangle.Width * WidthScale;
                var height = Size.Y.Value + Parent.ScreenRectangle.Height * HeightScale;
                var x = Position.X.Value;
                var y = Position.Y.Value;

                RelativeRectangle = new DrawRectangle(x, y, width, height);
                ScreenRectangle = GraphicsHelper.AlignRect(Alignment, RelativeRectangle, Parent.ScreenRectangle);
            }
            // Make it relative to the screen size.
            else
            {
                var width = Size.X.Value + WindowManager.VirtualScreen.X * WidthScale;
                var height = Size.Y.Value + WindowManager.VirtualScreen.Y * HeightScale;
                var x = Position.X.Value;
                var y = Position.Y.Value;

                RelativeRectangle = new DrawRectangle(x, y, width, height);
                ScreenRectangle = GraphicsHelper.AlignRect(Alignment, RelativeRectangle, WindowManager.Rectangle);
            }

            Children.ForEach(x => x.RecalculateRectangles());

            // Raise recalculated event.
            RectangleRecalculated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///
        /// </summary>
        protected abstract void DrawToSpriteBatch();

        /// <summary>
        ///     Resets the count of total drawn objects.
        ///     This is usually performed once every initial draw call.
        /// </summary>
        internal static void ResetTotalDrawnCount() => TotalDrawn = 0;

        /// <summary>
        ///     Performs all of the transformations in the queue.
        /// </summary>
        private void PerformTransformations(GameTime gameTime)
        {
            // Keep a list of transformations that are marked as done that'll be queued for removal.
            var queuedForDeletion = new List<Transformation>();

            foreach (var transformation in Transformations)
            {
                switch (transformation.Properties)
                {
                    case TransformationProperty.X:
                        X = transformation.PerformInterpolation(gameTime);
                        break;
                    case TransformationProperty.Y:
                        Y = transformation.PerformInterpolation(gameTime);
                        break;
                    case TransformationProperty.Width:
                        Width = transformation.PerformInterpolation(gameTime);
                        break;
                    case TransformationProperty.Height:
                        Height = transformation.PerformInterpolation(gameTime);
                        break;
                    case TransformationProperty.Alpha:
                        var type = GetType();

                        if (type == typeof(Sprite))
                        {
                            var sprite = (Sprite)this;
                            sprite.Alpha = transformation.PerformInterpolation(gameTime);
                        }
                        else if (type == typeof(SpriteText))
                        {
                            var spriteText = (SpriteText) this;
                            spriteText.Alpha = transformation.PerformInterpolation(gameTime);
                        }
                        break;
                    case TransformationProperty.Rotation:
                        if (GetType() == typeof(Sprite))
                        {
                            var sprite = (Sprite) this;
                            sprite.Rotation = transformation.PerformInterpolation(gameTime);
                        }
                        else
                            throw new NotImplementedException();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (transformation.Done)
                    queuedForDeletion.Add(transformation);
            }

            // Remove all completed transformations.
            queuedForDeletion.ForEach(x => Transformations.Remove(x));
        }
    }
}