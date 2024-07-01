using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Logging;

namespace Wobble.Graphics
{
    public abstract class Drawable3D
    {
        private Vector3 _size = Vector3.Zero;
        private Vector3 _position = Vector3.Zero;
        private Drawable3D _parent;
        private Vector3 _scale = Vector3.One;
        private Quaternion _rotation = Quaternion.Identity;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                RecalculateRectangles();
            }
        }

        public float X
        {
            get => Position.X;
            set => Position = new Vector3(value, Position.Y, Position.Z);
        }

        public float Y
        {
            get => Position.Y;
            set => Position = new Vector3(Position.X, value, Position.Z);
        }

        public float Z
        {
            get => Position.Z;
            set => Position = new Vector3(Position.X, Position.Y, value);
        }

        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                RecalculateRectangles();
            }
        }

        public List<Drawable3D> Children { get; set; } = new List<Drawable3D>();

        public Vector3 Size
        {
            get => _size;
            set
            {
                _size = value;
                RecalculateRectangles();
            }
        }

        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                RecalculateRectangles();
            }
        }

        public Vector3 ScaledWorldSize => WorldScale * Size;
        public Vector3 WorldPosition { get; private set; }

        public Vector3 WorldScale { get; private set; }

        public Drawable3D Parent
        {
            get => _parent;
            set
            {
                Parent?.Children.Remove(this);
                value?.Children.Add(this);
                _parent = value;
            }
        }

        public World World { get; set; }

        public BoundingBox WorldBoundingBox { get; set; }

        public BoundingBox RelativeBoundingBox { get; set; }

        /// <summary>
        ///     Recalculates the local and global rectangles of the object. Makes sure that the position
        ///     and sizes are relative to the parent if the drawable has one.
        /// </summary>
        protected void RecalculateRectangles()
        {
            // Make it relative to the parent.
            if (Parent != null)
            {
                WorldScale = Scale * Parent.WorldScale;
                RelativeBoundingBox = new BoundingBox(Vector3.Zero, Size * WorldScale);
                WorldPosition = Position + Parent.WorldBoundingBox.Min;
                WorldBoundingBox = new BoundingBox(WorldPosition, RelativeBoundingBox.Max + WorldPosition);
            }
            // Make it relative to the screen size.
            else
            {
                WorldScale = Scale;
                RelativeBoundingBox = new BoundingBox(Vector3.Zero, Size * Scale);
                WorldBoundingBox = new BoundingBox(Position, RelativeBoundingBox.Max + Position);
            }

            for (var i = 0; i < Children.Count; i++)
                Children[i].RecalculateRectangles();

            // Raise recalculated event.
            OnRectangleRecalculated();
        }

        /// <summary>
        ///
        /// </summary>
        protected virtual void OnRectangleRecalculated()
        {
        }

        // TODO Use WorldPosition
        public Matrix Transformation => Matrix.CreateTranslation(-Position) * Matrix.CreateScale(Scale) *
                                        Matrix.CreateFromQuaternion(Rotation);

        public bool Visible { get; set; } = true;

        /// <summary>
        ///    A list of updates that are scheduled to be run at the beginning of <see cref="Update"/>.
        ///    Should be used for scheduling UI updates from a separate thread.
        /// </summary>
        private List<Action> ScheduledUpdates { get; } = new List<Action>();

        protected Drawable3D()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            RunScheduledUpdates();

            // Update all of the contained children.
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                try
                {
                    //TotalDrawn++;
                    //Children[i].DrawOrder = TotalDrawn;
                    Children[i].Update(gameTime);
                }
                // Handle
                catch (ArgumentOutOfRangeException e)
                {
                    // In the event that a child was updated but the list was somehow modified
                    // just break out of the loop for now.
                    if (i < 0 || i >= Children.Count)
                        break;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Runs all updates that are scheduled for this drawable during <see cref="Update"/>
        /// </summary>
        protected void RunScheduledUpdates()
        {
            lock (ScheduledUpdates)
            {
                if (ScheduledUpdates.Count == 0)
                    return;

                var updates = new List<Action>(ScheduledUpdates);
                ScheduledUpdates.Clear();

                foreach (var update in updates)
                    update.Invoke();
            }
        }

        public virtual void Draw(GameTime gameTime, Camera camera)
        {
            if (!Visible) return;
            for (var i = 0; i < Children.Count; i++)
            {
                var drawable = Children[i];
                drawable.Draw(gameTime, camera);
            }
        }
    }
}