using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        public int Order { get; private set; }

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
        public DrawRectangle ScreenRectangle { get; private set; }

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        public DrawRectangle RelativeRectangle { get; private set; }

        /// <summary>
        ///     The position of the drawable
        /// </summary>
        private ScalableVector2 _position = new ScalableVector2();
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
        private ScalableVector2 _size = new ScalableVector2();
        public ScalableVector2 Size
        {
            get => _size;
            set
            {
                _size = value;
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
                Position.X.Value = value;
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
                Position.Y.Value = value;
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
                Size.X.Value = value;
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
                Size.X.Scale = value;
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
                Size.Y.Value = value;
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
                Size.Y.Scale = value;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
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
            Order = TotalDrawn;

            // Draw the children and set their order.
            foreach (var drawable in Children)
            {
                drawable.Draw(gameTime);

                TotalDrawn++;
                drawable.Order = TotalDrawn;
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
        public virtual void Dispose() => IsDisposed = true;

        /// <summary>
        ///     Recalculates the local and global rectangles of the object. Makes sure that the position
        ///     and sizes are relative to the parent if the drawable has one.
        /// </summary>
        private void RecalculateRectangles()
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
        }
    }
}