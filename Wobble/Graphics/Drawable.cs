using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;
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
        public int DrawOrder { get; set; }

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
                else if (DestroyIfParentIsNull)
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
        ///     Completely destroys the object if the parent is null
        /// </summary>
        public bool DestroyIfParentIsNull { get; set; } = true;

        /// <summary>
        ///     The children of this drawable. All children objects depend on this object's position
        ///     and size.
        /// </summary>
        public List<Drawable> Children { get; } = new List<Drawable>();

        /// <summary>
        ///     The drawable's rectangle relative to the entire screen.
        /// </summary>
        public RectangleF ScreenRectangle { get; private set; } = new RectangleF();

        /// <summary>
        ///     If outside this region, this will not be drawed.
        /// </summary>
        private RectangleF DrawRectangleMask { get; set; } =
            new RectangleF(0, 0, WindowManager.Width, WindowManager.Height);

        /// <summary>
        ///     Clipping region for children. Useful to RenderTargets
        /// </summary>
        protected virtual RectangleF ChildDrawRectangleMask =>
            Parent?.ChildDrawRectangleMask
            ?? new RectangleF(0, 0, WindowManager.Width, WindowManager.Height);

        /// <summary>
        ///     The bounding box of the drawable relative to the entire screen.
        /// </summary>
        public RectangleF ScreenMinimumBoundingRectangle { get; private set; } = new RectangleF();

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        public RectangleF RelativeRectangle { get; private set; }

        private RectangleF _alignedRelativeRectangle;

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        public RectangleF AlignedRelativeRectangle
        {
            get => _alignedRelativeRectangle;
            private set
            {
                _alignedRelativeRectangle = value;
                // Update _scaledAlignedRelativeRectangle
                // So that the rectangle is scaled with its position adjusted according to the pivot and scale
                _scaledAlignedRelativeRectangle = new RectangleF(
                    value.Position + Pivot * (Vector2.One - Scale) * value.Size,
                    value.Size * Scale);
            }
        }

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        private RectangleF _scaledAlignedRelativeRectangle;

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
                Pivot = Pivot;
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
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == Position.X.Value)
                    return;

                Position = new ScalableVector2(value, Position.Y.Value, Position.X.Scale, Position.Y.Scale);
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
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == Position.Y.Value)
                    return;

                Position = new ScalableVector2(Position.X.Value, value, Position.X.Scale, Position.Y.Scale);
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

                if (AutoScaleWidth)
                    value += WindowManager.VirtualScreen.X - WindowManager.BaseResolution.X;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == Size.X.Value)
                    return;

                Size = new ScalableVector2(value, Size.Y.Value, Size.X.Scale, Size.Y.Scale);
                Pivot = Pivot;
            }
        }

        /// <summary>
        ///     The width scale of the object.
        /// </summary>
        public float WidthScale
        {
            get => Size.X.Scale;
            set => Size = new ScalableVector2(Size.X.Value, Size.Y.Value, value, Size.Y.Scale);
        }

        /// <summary>
        ///     The total width of the drawable, considering the width scale
        /// </summary>
        private float RelativeWidth =>
            Size.X.Value + (Parent?.ScreenRectangle.Width ?? WindowManager.VirtualScreen.X) * WidthScale;

        /// <summary>
        ///     The total height of the drawable, considering the height scale
        /// </summary>
        private float RelativeHeight =>
            Size.Y.Value + (Parent?.ScreenRectangle.Height ?? WindowManager.VirtualScreen.Y) * HeightScale;

        private Vector2 _scale = Vector2.One;

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                RecalculateRectangles();
            }
        }

        public Vector2 AbsoluteScale { get; private set; }

        /// <summary>
        ///     The height of the object.
        /// </summary>
        public float Height
        {
            get => Size.Y.Value;
            set
            {
                value = MathHelper.Clamp(value, 0, int.MaxValue);

                if (AutoScaleHeight)
                    value += WindowManager.VirtualScreen.Y - WindowManager.BaseResolution.Y;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == Size.Y.Value)
                    return;

                Size = new ScalableVector2(Size.X.Value, value, Size.X.Scale, Size.Y.Scale);
                Pivot = Pivot;
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

        private Vector2 _pivot = new Vector2(0.5f, 0.5f);

        /// <summary>
        ///     The pivot about which the rotation will be performed.
        ///     (0, 0) corresponds to the top-left corner, (1, 1) bottom right.
        /// </summary>
        public Vector2 Pivot
        {
            get => _pivot;
            set
            {
                _pivot = value;
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
            set
            {
                _rotation = value;
                RecalculateRectangles();
            }
        }

        public float AbsoluteRotation { get; private set; }

        /// <summary>
        ///     Applying this to <see cref="AlignedRelativeRectangle"/> gives the screen space position
        /// </summary>
        protected Matrix2D ChildPositionTransform { get; set; } = Matrix2D.Identity;

        /// <summary>
        ///     A transform that rotates the relative coordinates about the pivot
        ///     Applying this to <see cref="AlignedRelativeRectangle"/> gives the relative coordinate after rotation.
        /// </summary>
        protected Matrix2D ChildRelativeTransform { get; set; } = Matrix2D.Identity;

        protected virtual void RecalculateTransformMatrix()
        {
            var relativeOrigin = Pivot * new Vector2(RelativeWidth, RelativeHeight);

            // Move so the origin is at RelativeOrigin, rotate, then move back
            ChildRelativeTransform = Matrix2D.CreateTranslation(-relativeOrigin)
                                     * Matrix2D.CreateRotationZ(Rotation)
                                     * Matrix2D.CreateTranslation(relativeOrigin);

            // Rotate relative coordinates first, then add our own relative position
            // Finally, apply our parent's transformation
            ChildPositionTransform = ChildRelativeTransform
                                     * Matrix2D.CreateScale(Scale)
                                     * Matrix2D.CreateTranslation(_scaledAlignedRelativeRectangle.Position)
                                     * (Parent?.ChildPositionTransform ?? Matrix2D.Identity);
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
        ///     Determines whether the width will be automatically scaled according to
        ///     <see cref="WindowManager.BaseResolution"/> and <see cref="WindowManager.VirtualScreen"/>
        /// </summary>
        private bool _autoScaleWidth;
        public bool AutoScaleWidth
        {
            get => _autoScaleWidth;
            set
            {
                _autoScaleWidth = value;
                Width = Width;
            }
        }

        /// <summary>
        ///     Determines whether the height will be automatically scaled according to
        ///     <see cref="WindowManager.BaseResolution"/> and <see cref="WindowManager.VirtualScreen"/>
        /// </summary>
        private bool _autoScaleHeight;
        public bool AutoScaleHeight
        {
            get => _autoScaleHeight;
            set
            {
                _autoScaleHeight = value;
                Height = Height;
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
                {
                    for (var i = 0; i < Children.Count; i++)
                        Children[i].Visible = value;
                }
            }
        }

        /// <summary>
        /// </summary>
        public float AnimationWaitTime { get; private set; }

        /// <summary>
        ///     Dictates whether or not when we set the visibility of this object, the children's
        ///     get set as well.
        /// </summary>
        public bool SetChildrenVisibility { get; set; }

        /// <summary>
        ///     If the drawable will still draw even if it is off-screen
        /// </summary>
        public bool DrawIfOffScreen { get; set; }

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
        ///     The list of animations to perform on this drawable.
        /// </summary>
        public List<Animation> Animations { get; } = new List<Animation>();

        /// <summary>
        ///     A list of completed animations to remove at the start of a frame
        /// </summary>
        private List<Animation> AnimationsToRemove { get; } = new List<Animation>();

        /// <summary>
        ///    The border around the drawable.
        /// </summary>
        public PrimitiveLineBatch Border { get; private set; }

        /// <summary>
        ///    A list of updates that are scheduled to be run at the beginning of <see cref="Update"/>.
        ///    Should be used for scheduling UI updates from a separate thread.
        /// </summary>
        private List<Action> ScheduledUpdates { get; } = new List<Action>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            RunScheduledUpdates();
            PerformTransformations(gameTime);

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            if (!RectangleF.Intersects(ScreenMinimumBoundingRectangle, DrawRectangleMask) && !DrawIfOffScreen)
                return;

            // Draw the children and set their order.
            // Increase the total amount of drawables that were drawn and set the order to the current
            // total.
            TotalDrawn++;
            DrawOrder = TotalDrawn;

            try
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    var drawable = Children[i];
                    drawable.Draw(gameTime);

                    TotalDrawn++;
                    drawable.DrawOrder = TotalDrawn;
                }
            }
            // In the case of modifying a drawable collection, an InvalidOperationException might occur
            catch (InvalidOperationException e)
            {
                if (!e.Message.Contains("Collection was modified; enumeration operation may not execute."))
                    throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Destroys the object. Removes the parent object. Any derivates should
        ///     free any resources used by the object.
        /// </summary>
        public virtual void Destroy()
        {
            Dispose();
            DestroyIfParentIsNull = true;
            Parent = null;
        }

        /// <summary>
        ///     Adds a border to the drawable.
        /// </summary>
        public void AddBorder(Color color, float thickness = 1)
        {
            if (Border != null)
                return;

            Border = new PrimitiveLineBatch(new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(Width, 0),
                new Vector2(Width, Height),
                new Vector2(0, Height),
                new Vector2(0, 0)
            }, thickness)
            {
                Alignment = Alignment.TopLeft,
                Parent = this,
                Tint = color,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Dispose() => IsDisposed = true;

        /// <summary>
        ///     Recalculates the local and global rectangles of the object. Makes sure that the position
        ///     and sizes are relative to the parent if the drawable has one.
        /// </summary>
        protected void RecalculateRectangles()
        {
            // Update AbsoluteRotation
            AbsoluteRotation = (Parent?.AbsoluteRotation ?? 0) + Rotation;
            AbsoluteScale = (Parent?.AbsoluteScale ?? Vector2.One) * Scale;
            DrawRectangleMask = Parent?.ChildDrawRectangleMask
                                ?? new RectangleF(0, 0, WindowManager.Width, WindowManager.Height);

            // Make it relative to the parent.
            var width = RelativeWidth;
            var height = RelativeHeight;
            var x = Position.X.Value;
            var y = Position.Y.Value;

            RelativeRectangle = new RectangleF(x, y, width, height);
            if (Parent != null)
            {
                AlignedRelativeRectangle =
                    GraphicsHelper.AlignRect(Alignment, RelativeRectangle, Parent.RelativeRectangle, true);
                ScreenRectangle = GraphicsHelper.Transform(_scaledAlignedRelativeRectangle, Parent.ChildPositionTransform);
            }
            // Make it relative to the screen size.
            else
            {
                AlignedRelativeRectangle = GraphicsHelper.AlignRect(Alignment, RelativeRectangle, WindowManager.Rectangle, true);
                ScreenRectangle = GraphicsHelper.Offset(_scaledAlignedRelativeRectangle, WindowManager.Rectangle);
            }

            // Update the matrix, now that we have AlignedRelativeRectangle calculated
            // Note that this calculation of AlignedRelativeRectangle and ScreenRectangle relies on the parent's
            // transform, and the parent's matrices are calculated before RecalculateRectangles() is called, had there
            // been an update to the parent.
            RecalculateTransformMatrix();

            var relativeBoundingRectangle =
                GraphicsHelper.MinimumBoundingRectangle(ScreenRectangle, AbsoluteRotation, true);

            // Recalculate the border points.
            if (Border != null)
            {
                Border.Points = new List<Vector2>()
                {
                    new Vector2(relativeBoundingRectangle.Left, relativeBoundingRectangle.Top),
                    new Vector2(relativeBoundingRectangle.Right, relativeBoundingRectangle.Top),
                    new Vector2(relativeBoundingRectangle.Right, relativeBoundingRectangle.Bottom),
                    new Vector2(relativeBoundingRectangle.Left, relativeBoundingRectangle.Bottom),
                    new Vector2(relativeBoundingRectangle.Left, relativeBoundingRectangle.Top)
                };
            }

            // (0, 0) * ChildPositionTransform + relativeBoundingRectangle.Position
            // TopLeft absolute coordinate offset by the top left of relative bounding rect
            // gives the screen bounding rect
            ScreenMinimumBoundingRectangle =
                new RectangleF(
                    ChildPositionTransform.Translation + relativeBoundingRectangle.Position,
                    relativeBoundingRectangle.Size);

            for (var i = 0; i < Children.Count; i++)
                Children[i].RecalculateRectangles();

            // Raise recalculated event.
            OnRectangleRecalculated();
        }

        /// <summary>
        /// </summary>
        public abstract void DrawToSpriteBatch();

        /// <summary>
        ///
        /// </summary>
        protected virtual void OnRectangleRecalculated()
        {
        }

        /// <summary>
        ///     Resets the count of total drawn objects.
        ///     This is usually performed once every initial draw call.
        /// </summary>
        internal static void ResetTotalDrawnCount() => TotalDrawn = 0;

        /// <summary>
        ///     Performs all of the Animations in the queue.
        /// </summary>
        public void PerformTransformations(GameTime gameTime)
        {
            for (var i = 0; i < AnimationsToRemove.Count; i++)
                Animations.Remove(AnimationsToRemove[i]);

            if (AnimationsToRemove.Count != 0)
                AnimationsToRemove.Clear();

            for (var i = 0; i < Animations.Count; i++)
            {
                var animation = Animations[i];
                try
                {
                    var breakOutOfLoop = false;

                    switch (animation.Properties)
                    {
                        case AnimationProperty.Wait:
                            if (animation != Animations[0])
                            {
                                breakOutOfLoop = true;
                                break;
                            }

                            AnimationWaitTime = animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.X:
                            X = (int) animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Y:
                            Y = (int) animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Width:
                            Width = (int) animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Height:
                            Height = (int) animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Alpha:
                            var type = GetType();

                            if (this is Sprite)
                            {
                                var sprite = (Sprite) this;
                                sprite.Alpha = animation.PerformInterpolation(gameTime);
                            }

                            break;
                        case AnimationProperty.Rotation:
                            if (this is Sprite)
                            {
                                var sprite = (Sprite) this;
                                sprite.Rotation = animation.PerformInterpolation(gameTime);
                            }
                            else
                                throw new NotImplementedException();

                            break;
                        case AnimationProperty.Color:
                            if (this is Sprite)
                            {
                                var sprite = (Sprite) this;
                                sprite.Tint = animation.PerformColorInterpolation(gameTime);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (animation.Properties == AnimationProperty.Wait && !animation.Done || breakOutOfLoop)
                        break;

                    if (animation.Done)
                    {
                        AnimationsToRemove.Add(animation);

                        if (animation.Properties == AnimationProperty.Wait)
                        {
                            AnimationWaitTime = 0;

                            for (var j = 0; j < Animations.Count; j++)
                            {
                                var a = Animations[j];
                                switch (a.Properties)
                                {
                                    case AnimationProperty.X:
                                        a.Start = X;
                                        break;
                                    case AnimationProperty.Y:
                                        a.Start = Y;
                                        break;
                                    case AnimationProperty.Width:
                                        a.Start = Width;
                                        break;
                                    case AnimationProperty.Height:
                                        a.Start = Height;
                                        break;
                                    case AnimationProperty.Rotation:
                                        a.Start = Rotation;
                                        break;
                                    case AnimationProperty.Alpha:
                                        var type = GetType();

                                        if (this is Sprite)
                                        {
                                            var sprite = (Sprite) this;
                                            a.Start = sprite.Alpha;
                                        }

                                        break;
                                    case AnimationProperty.Color:
                                        if (this is Sprite)
                                        {
                                            var sprite = (Sprite) this;
                                            a.StartColor = sprite.Tint;
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    break;
                }
            }
        }

        /// <summary>
        ///     Removes all animations from the drawable
        /// </summary>
        public void ClearAnimations()
        {
            lock (Animations)
                Animations.Clear();
        }

        /// <summary>
        ///     Returns if the Drawable is currently hovered
        /// </summary>
        /// <returns></returns>
        public bool IsHovered() => GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position);

        /// <summary>
        ///     Removes all previously scheduled updates, and schedules a new one to run in the next frame
        /// </summary>
        /// <param name="action"></param>
        public void ScheduleUpdate(Action action)
        {
            lock (ScheduledUpdates)
            {
                ScheduledUpdates.Clear();
                ScheduledUpdates.Add(action);
            }
        }

        /// <summary>
        ///     Schedules a new update to be run in the next frame, but does not remove previously scheduled updates.
        /// </summary>
        /// <param name="action"></param>
        public void AddScheduledUpdate(Action action)
        {
            lock (ScheduledUpdates)
                ScheduledUpdates.Add(action);
        }

        /// <summary>
        ///     Removes all
        /// </summary>
        public void RemoveScheduledUpdates()
        {
            lock (ScheduledUpdates)
                ScheduledUpdates.Clear();
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

        /// <summary>
        ///     Moves the drawable to an x position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        public Drawable MoveToX(float x, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.X, easingType, X, x, time));

            return this;
        }

        /// <summary>
        ///     Moves the drawable to a y position
        /// </summary>
        /// <param name="y"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        public Drawable MoveToY(int y, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.Y, easingType, Y, y, time));

            return this;
        }

        /// <summary>
        ///     Moves the drawable to a given position
        /// </summary>
        public Drawable MoveToPosition(Vector2 position, Easing easingType, int time)
        {
            lock (Animations)
            {
                Animations.Add(new Animation(AnimationProperty.X, easingType, X, position.X, time));
                Animations.Add(new Animation(AnimationProperty.Y, easingType, Y, position.Y, time));
            }

            return this;
        }

        /// <summary>
        ///     Animate's the drawable's height
        /// </summary>
        /// <param name="height"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        public Drawable ChangeHeightTo(int height, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.Height, easingType, Height, height, time));

            return this;
        }

        /// <summary>
        ///     Animates the drawable's width
        /// </summary>
        /// <param name="width"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        public Drawable ChangeWidthTo(int width, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.Width, easingType, Width, width, time));

            return this;
        }

        /// <summary>
        ///     Animate's the drawable's width and height.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        public Drawable ChangeSizeTo(Vector2 size, Easing easingType, int time)
        {
            lock (Animations)
            {
                Animations.Add(new Animation(AnimationProperty.Width, easingType, Width, size.X, time));
                Animations.Add(new Animation(AnimationProperty.Height, easingType, Height, size.Y, time));
            }

            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual Drawable Wait(int time = 0)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.Wait, Easing.Linear, 0, time, time));

            return this;
        }

        ~Drawable()
        {
            if (IsDisposed)
                return;

            Destroy();
            IsDisposed = true;
        }
    }
}
