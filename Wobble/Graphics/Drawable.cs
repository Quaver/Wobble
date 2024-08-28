using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
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
        ///     Represents the rotation, position, scale, and origin of the quad we're drawing
        /// </summary>
        public virtual QuadTransform Transform { get; } = new QuadTransform();

        /// <summary>
        ///     The main render target to render to.
        /// </summary>
        public RenderTargetOptions RenderTargetOptions { get; } = new RenderTargetOptions();

        /// <summary>
        ///     A projection sprite that has the same dimension, position, rotation and parent as the container.
        ///     It shows <see cref="RenderTarget"/>, which the container can render its entire content to
        /// </summary>
        public RenderProjectionSprite DefaultProjectionSprite { get; private set; }

        /// <summary>
        ///     The total amount of drawables that are drawn on-screen.
        /// </summary>
        public static int TotalDrawn { get; private set; }

        /// <summary>
        ///     The order of which this object was drawn. Higher means the object was drawn later.
        /// </summary>
        public int DrawOrder { get; set; }

        /// <summary>
        ///     A faster way of null checking render target options
        /// </summary>
        private bool _isCasting;

        /// <summary>
        ///     The parent of this drawable in which it depends on for its position and size.
        /// </summary>
        private Drawable _parent;
        public Drawable Parent
        {
            get => _parent;
            set
            {
                Transform.Parent = value?.Transform;
                // If this drawable previously had a parent, remove it from the old parent's list
                // of children.
                _parent?.Children.Remove(this);

                _parent = value;

                // If we do end up having a non-null value for the new parent, we'll want to
                // add this drawable to their list of children.
                if (value != null)
                {
                    value.Children.Add(this);

                    // Derive layer from our parent
                    if (value.SetChildrenLayer)
                        Layer = value.Layer;
                }
                else if (DestroyIfParentIsNull)
                {
                    // If we've received null for the parent however, that must mean we want to FULLY
                    // destroy and dispose of the object.
                    for (var i = Children.Count - 1; i >= 0; i--)
                        Children[i].Destroy();

                    Children.Clear();
                }

                // When both are null, we implicitly add this to the Default layer
                if (value == null && (Layer == null || IsDisposed))
                    Layer = null;

                RecalculateRectangles();
            }
        }

        private Layer _layer;

        /// <summary>
        ///     Whether to set children's layer too when <see cref="Layer"/> changes
        /// </summary>
        public bool SetChildrenLayer { get; set; } = false;

        /// <summary>
        ///     Layer of this drawable. If null, it will be drawn over the Default layer.
        /// </summary>
        public Layer Layer
        {
            get => _layer;
            set
            {
                if (_layer == value && _layer != null)
                    return;

                _layer?.RemoveDrawable(this);
                value?.AddDrawable(this);
                _layer = value;

                if (!SetChildrenLayer)
                    return;

                foreach (var child in Children)
                {
                    child.Layer = value;
                }
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
        protected virtual RectangleF ChildDrawRectangleMask { get; set; }

        /// <summary>
        ///     The bounding box of the drawable relative to the entire screen.
        /// </summary>
        public RectangleF ScreenMinimumBoundingRectangle { get; private set; } = new RectangleF();

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        public RectangleF RelativeRectangle { get; private set; }

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

        public event EventHandler<ScalableVector2> SizeChanged;
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
                SizeChanged?.Invoke(this, value);
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
        ///     The relative Z position of the object
        /// </summary>
        public float Z
        {
            get => Transform.Position.Z;
            set => Transform.Position = new Vector3(Transform.Position.X, Transform.Position.Y, value);
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
            _size.X.Value + (Parent?.ScreenRectangle.Width ?? WindowManager.VirtualScreen.X) * _size.X.Scale;

        /// <summary>
        ///     The total height of the drawable, considering the height scale
        /// </summary>
        private float RelativeHeight =>
            _size.Y.Value + (Parent?.ScreenRectangle.Height ?? WindowManager.VirtualScreen.Y) * _size.Y.Scale;

        private Vector2 _scale = Vector2.One;

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                Transform.Scale = new Vector3(value, 1);
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
                var origin = _pivot * RelativeRectangle.Size;
                Transform.Origin = new Vector3(origin, 0);
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
                Transform.Rotation = Quaternion.CreateFromAxisAngle(RotationAxis, value);
            }
        }

        /// <summary>
        ///     The axis of rotation
        /// </summary>
        private Vector3 _rotationAxis = Vector3.UnitZ;

        public Vector3 RotationAxis
        {
            get => _rotationAxis;
            set
            {
                _rotationAxis = value;
                Transform.Rotation = Quaternion.CreateFromAxisAngle(value, Rotation);
            }
        }

        /// <summary>
        ///     The underlying quaternion that represents the rotation.
        ///     Changing this will not change <see cref="RotationAxis"/> and <see cref="Rotation"/>,
        ///     so be careful.
        /// </summary>
        private Quaternion RotationQuaternion
        {
            get => Transform.Rotation;
            set => Transform.Rotation = value;
        }

        public float AbsoluteRotation { get; private set; }

        /// <summary>
        ///     The final color to be drawn on screen when calling <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch"/> draws.
        ///     This color is affected by <see cref="Sprite.RelativeColor"/>.
        /// </summary>
        protected Color AbsoluteColor { get; private set; } = Color.White;

        /// <summary>
        ///     The base color of its children. This excludes the effect from <see cref="Sprite.RelativeColor"/>.
        /// </summary>
        private Vector4 ChildrenColor { get; set; } = Vector4.One;

        private float _uiAlpha = 1;

        /// <summary>
        ///     Sets the alpha for itself, while multiplying the alpha to all of its children and consequently descendents
        /// </summary>
        public float UIAlpha
        {
            get => _uiAlpha;
            set
            {
                _uiAlpha = value;
                RecalculateColor();
            }
        }

        private Color _uiTint = Color.White;

        /// <summary>
        ///     Sets the tint for itself, while multiplying the tint to all of its children and consequently descendents
        /// </summary>
        public Color UITint
        {
            get => _uiTint;
            set
            {
                _uiTint = value;
                RecalculateColor();
            }
        }

        /// <summary>
        ///     Additional color applied to the drawable, overridable.
        ///     This is overridden in <see cref="Sprite"/> for compatibility.
        /// </summary>
        protected virtual Color RelativeColor => Color.White;

        /// <summary>
        ///     Updates the <see cref="ChildrenColor"/> and <see cref="AbsoluteColor"/> of itself only
        /// </summary>
        protected void RecalculateSelfColor()
        {
            var parentChildrenColor = Parent?.ChildrenColor ?? Vector4.One;
            ChildrenColor = (_uiTint * _uiAlpha).ToVector4() * parentChildrenColor;

            // RelativeColor affects the final color but not the children's color
            AbsoluteColor = new Color(ChildrenColor * RelativeColor.ToVector4());
        }

        /// <summary>
        ///     Updates the <see cref="ChildrenColor"/> and <see cref="AbsoluteColor"/> of itself and all of its children,
        ///     recursively.
        /// </summary>
        /// <seealso cref="RecalculateSelfColor"/>
        protected void RecalculateColor()
        {
            RecalculateSelfColor();

            try
            {
                foreach (var child in Children)
                {
                    child.RecalculateColor();
                }
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Applying this to <see cref="AlignedRelativeRectangle"/> gives the screen space position
        /// </summary>
        private Matrix2 _childPositionTransform = Matrix2.Identity;

        /// <summary>
        ///     A transform that rotates the relative coordinates about the pivot
        ///     Applying this to <see cref="AlignedRelativeRectangle"/> gives the relative coordinate after rotation.
        /// </summary>
        private Matrix2 _childRelativeTransform = Matrix2.Identity;

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

                    if (drawable.Layer != null)
                        continue;

                    drawable.WrappedDraw(gameTime);

                    TotalDrawn++;
                    drawable.DrawOrder = TotalDrawn;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        public void WrappedDraw(GameTime gameTime)
        {
            if (!_isCasting)
            {
                Draw(gameTime);
                return;
            }

            if (Parent == null)
                DefaultProjectionSprite?.Draw(gameTime);
            GameBase.Game.ScheduledRenderTargetDraws.Add(DrawToRenderTarget);
        }


        /// <summary>
        ///     Draw this container to a render target so its view can be duplicated and shown in
        ///     a different way.
        ///     **THIS CAN CAUSE PERFORMANCE DEGREDATION**
        /// </summary>
        /// <remarks>
        ///     The render target is bounded by the size of the container, so
        ///     anything outside this container will be clipped
        /// </remarks>
        /// <param name="projectDefault">Whether a sprite will be spawned to show the container as normal</param>
        public void CastToRenderTarget(bool projectDefault = true)
        {
            _isCasting = true;
            RenderTargetOptions.ContainerRectangleSize =
                new Point((int)RelativeRectangle.Size.Width, (int)RelativeRectangle.Size.Height);
            RenderTargetOptions.Enabled = true;
            RecalculateRectangles();

            DefaultProjectionSprite?.Destroy();

            if (projectDefault)
            {
                DefaultProjectionSprite = new RenderProjectionSprite
                {
                    Size = Size,
                    Position = Position,
                    Rotation = Rotation,
                    Alignment = Alignment,
                    Parent = Parent
                };
                DefaultProjectionSprite.BindProjectionContainer(this);
            }
        }

        public void StopCasting()
        {
            _isCasting = false;
            DefaultProjectionSprite?.Destroy();
            RenderTargetOptions.Enabled = false;
            DefaultProjectionSprite = null;
            RecalculateRectangles();
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

        public void RecalculateDrawMask()
        {
            DrawRectangleMask = Parent?.ChildDrawRectangleMask
                                ?? new RectangleF(0, 0, WindowManager.Width, WindowManager.Height);
            ChildDrawRectangleMask = _isCasting ? RenderTargetOptions.RenderTarget.Value.Bounds : DrawRectangleMask;
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
            if (_isCasting)
            {
                AbsoluteRotation = 0;
                AbsoluteScale = RenderTargetOptions.Scale;
            }
            else
            {
                // Update AbsoluteRotation
                AbsoluteRotation = (Parent?.AbsoluteRotation ?? 0) + Rotation;
                AbsoluteScale = (Parent?.AbsoluteScale ?? Vector2.One) * Scale;
            }

            RecalculateDrawMask();
            RecalculateSelfColor();

            // Make it relative to the parent.
            var width = RelativeWidth;
            var height = RelativeHeight;
            var x = Position.X.Value;
            var y = Position.Y.Value;

            RelativeRectangle = new RectangleF(x, y, width, height);
            if (Parent != null)
            {
                RelativeRectangle =
                    GraphicsHelper.AlignRect(Alignment, RelativeRectangle, Parent.RelativeRectangle, true);
            }
            // Make it relative to the screen size.
            else
            {
                RelativeRectangle =
                    GraphicsHelper.AlignRect(Alignment, RelativeRectangle, WindowManager.Rectangle, true);
            }

            Pivot = Pivot;
            Transform.Scale = new Vector3(Scale, 1);
            Transform.Position = new Vector3(RelativeRectangle.Position, Z);

            var worldPos = Transform.WorldPosition;
            var worldScale = Transform.WorldScale;
            var worldSize = new Vector2(RelativeRectangle.Width * worldScale.X, RelativeRectangle.Height * worldScale.Y);
            ScreenRectangle = new RectangleF(worldPos.X, worldPos.Y, worldSize.X, worldSize.Y);

            // Update the matrix, now that we have AlignedRelativeRectangle calculated
            // Note that this calculation of AlignedRelativeRectangle and ScreenRectangle relies on the parent's
            // transform, and the parent's matrices are calculated before RecalculateRectangles() is called, had there
            // been an update to the parent.

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
                    new Vector2(worldPos.X, worldPos.Y) + relativeBoundingRectangle.Position,
                    relativeBoundingRectangle.Size);

            for (var i = 0; i < Children.Count; i++)
                Children[i].RecalculateRectangles();

            if (DefaultProjectionSprite != null)
            {
                DefaultProjectionSprite.Parent = Parent;
                DefaultProjectionSprite.Size = Size;
                DefaultProjectionSprite.Scale = Scale;
                DefaultProjectionSprite.Rotation = Rotation;
                DefaultProjectionSprite.Position = Position;
                DefaultProjectionSprite.Alignment = Alignment;
            }

            RenderTargetOptions.ContainerRectangleSize =
                new Point((int)RelativeRectangle.Width, (int)RelativeRectangle.Height);
            RenderTargetOptions.ResetRenderTarget();

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

        private void DrawToRenderTarget(GameTime gameTime)
        {
            if (!_isCasting)
                return;

            GameBase.Game.TryEndBatch();
            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTargetOptions.RenderTarget.Value);
            GameBase.Game.GraphicsDevice.Clear(RenderTargetOptions.BackgroundColor);

            Draw(gameTime);

            // Attempt to end the spritebatch
            _ = GameBase.Game.TryEndBatch();
        }

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
                            X = (int)animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Y:
                            Y = (int)animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Width:
                            Width = (int)animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Height:
                            Height = (int) animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.UIAlpha:
                            UIAlpha = animation.PerformInterpolation(gameTime);
                            break;
                        case AnimationProperty.Alpha:
                            var type = GetType();

                            if (this is Sprite)
                            {
                                var sprite = (Sprite)this;
                                sprite.Alpha = animation.PerformInterpolation(gameTime);
                            }

                            break;
                        case AnimationProperty.Rotation:
                            if (this is Sprite)
                            {
                                var sprite = (Sprite)this;
                                sprite.Rotation = animation.PerformInterpolation(gameTime);
                            }
                            else
                                throw new NotImplementedException();

                            break;
                        case AnimationProperty.Color:
                            if (this is Sprite)
                            {
                                var sprite = (Sprite)this;
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
                                    case AnimationProperty.UIAlpha:
                                        a.Start = UIAlpha;
                                        break;
                                    case AnimationProperty.Alpha:
                                        var type = GetType();

                                        if (this is Sprite)
                                        {
                                            var sprite = (Sprite)this;
                                            a.Start = sprite.Alpha;
                                        }

                                        break;
                                    case AnimationProperty.Color:
                                        if (this is Sprite)
                                        {
                                            var sprite = (Sprite)this;
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
        public bool IsHovered() =>
            GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position);

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
        /// <param name="alpha"></param>
        /// <param name="easingType"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Drawable UIFadeTo(float alpha, Easing easingType, int time)
        {
            lock (Animations)
                Animations.Add(new Animation(AnimationProperty.UIAlpha, easingType, UIAlpha, alpha, time));

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