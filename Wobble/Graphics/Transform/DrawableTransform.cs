using System;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Wobble.Window;

namespace Wobble.Graphics.Transform
{
    /// <summary>
    ///     Represents the position, rotation, and scale of a three-dimensional game object.
    /// </summary>
    /// <seealso cref="BaseTransform{TMatrix}" />
    /// <remarks>
    ///     <para>
    ///         Every game object has a transform which is used to store and manipulate the position, rotation and scale
    ///         of the object. Every transform can have a parent, which allows to apply position, rotation and scale to game
    ///         objects hierarchically.
    ///     </para>
    /// </remarks>
    public class DrawableTransform : WobbleBaseTransform<Transform3D>
    {
        private Vector3 _calculatedPosition;
        private Vector3 _rotationAxis = Vector3.UnitZ;
        private float _rotationAngle;
        private Quaternion _selfRotation = Quaternion.Identity;
        private Vector2 _scale = Vector2.One;
        private Vector2 _pivot = new(0.5f, 0.5f);
        private Vector3 _origin = new(0.5f, 0.5f, 0.5f);
        private ScalableVector2 _size;
        private ScalableVector2 _position;
        private Alignment _alignment = Alignment.TopLeft;
        private bool _independentRotation;
        private Transform3D _selfLocalMatrix;
        private Transform3D _childLocalMatrix;
        private Transform3D _selfWorldMatrix;
        private Transform3D _childWorldMatrix;
        private BitVector32 _selfFlags;
        private static readonly int SelfMatrixOverride = BitVector32.CreateMask();
        private static readonly int SelfRotationSet = BitVector32.CreateMask(SelfMatrixOverride);
        private BitVector32 _childFlags;
        private static readonly int ChildMatrixOverride = BitVector32.CreateMask();

        /// <summary>
        ///     top left, top right, bottom left, bottom right
        /// </summary>
        private readonly Vector3[] sourceVertices = new Vector3[4];

        private readonly Vector3[] _vertices = new Vector3[4];

        #region World

        public Quaternion SelfRotation
        {
            get => _selfRotation;
            set
            {
                _selfRotation = value;
                _selfFlags[SelfRotationSet] = value != Quaternion.Identity;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public override Transform3D SelfWorldMatrix
        {
            get
            {
                if (_selfFlags.Data == 0)
                    return WorldMatrix;

                RecalculateWorldMatrixIfNecessary();
                return _selfWorldMatrix;
            }
            protected set
            {
                _selfWorldMatrix = value;
                RecalculateVertices();
            }
        }

        public override Transform3D ChildWorldMatrix
        {
            get
            {
                if (_childFlags.Data == 0)
                    return WorldMatrix;

                RecalculateWorldMatrixIfNecessary();
                return _childWorldMatrix;
            }
            protected set => _childWorldMatrix = value;
        }

        public Vector3[] Vertices
        {
            get
            {
                RecalculateWorldMatrixIfNecessary();
                return _vertices;
            }
        }

        /// <summary>
        ///     Gets the world position.
        /// </summary>
        /// <value>
        ///     The world position.
        /// </value>
        public Vector3 WorldPosition => WorldMatrix.Translation;

        /// <summary>
        ///     Gets the world scale.
        /// </summary>
        /// <value>
        ///     The world scale.
        /// </value>
        public Vector3 WorldScale
        {
            get
            {
                WorldMatrix.Decompose(out _, out _, out var scale);
                return scale;
            }
        }


        /// <summary>
        ///     Gets the world rotation quaternion in radians.
        /// </summary>
        /// <value>
        ///     The world rotation quaternion in radians.
        /// </value>
        public Quaternion WorldRotation
        {
            get
            {
                WorldMatrix.Decompose(out _, out var rotation, out _);
                return rotation;
            }
        }

        #endregion

        #region Local

        public Alignment Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                RecalculatePosition();
            }
        }

        public ScalableVector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                RecalculatePosition();
            }
        }

        public ScalableVector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                RecalculatePosition();
            }
        }

        public float Z
        {
            get => _calculatedPosition.Z;
            set
            {
                _calculatedPosition.Z = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        ///     Gets or sets the local position.
        /// </summary>
        /// <value>
        ///     The local position.
        /// </value>
        public Vector3 CalculatedPosition
        {
            get => _calculatedPosition;
            protected set
            {
                _calculatedPosition = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public float RotationAngle
        {
            get => _rotationAngle;
            set
            {
                _rotationAngle = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public Vector3 RotationAxis
        {
            get => _rotationAxis;
            set
            {
                _rotationAxis = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        ///     Gets or sets the local rotation quaternion in radians.
        /// </summary>
        /// <value>
        ///     The local rotation quaternion in radians.
        /// </value>
        public Quaternion Rotation
        {
            get { return Quaternion.CreateFromAxisAngle(_rotationAxis, _rotationAngle); }
            set
            {
                _rotationAngle = MathF.Acos(value.W) * 2;
                _rotationAxis = new Vector3(value.X, value.Y, value.Z);
                _rotationAxis.Normalize();
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        ///     Gets or sets the local scale.
        /// </summary>
        /// <value>
        ///     The local scale.
        /// </value>
        public Vector2 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        ///     Gets or sets the local scale.
        /// </summary>
        /// <value>
        ///     The local scale.
        /// </value>
        public Vector3 Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                _pivot = new Vector2(_origin.X / RelativeRectangle.Width, _origin.Y / RelativeRectangle.Height);
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        ///     If true, cancels all rotation effects from the parent.
        /// </summary>
        public bool IndependentRotation
        {
            get => _independentRotation;
            set
            {
                _independentRotation = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public Vector2 Pivot
        {
            get => _pivot;
            set
            {
                _pivot = value;
                _origin = new Vector3(RelativeRectangle.Size * _pivot, 0.5f);
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        #endregion

        private bool NeedsSelfTransformCalculation => _selfFlags.Data != 0 && !_selfFlags[SelfMatrixOverride];

        private bool NeedsChildTransformCalculation => _childFlags.Data != 0 && !_childFlags[ChildMatrixOverride];

        public DrawableTransform()
        {
            ParentChanged += OnParentChanged;
            TransformUpdated += RecalculateVertices;
        }

        public event Action PositionRecalculated;

        /// <summary>
        ///     The drawable's rectangle relative to the entire screen.
        /// </summary>
        public RectangleF ScreenRectangle { get; private set; }

        /// <summary>
        ///     The bounding box of the drawable relative to the entire screen.
        /// </summary>
        public RectangleF ScreenMinimumBoundingRectangle { get; private set; }

        /// <summary>
        ///     The rectangle relative to the drawable's parent.
        /// </summary>
        public RectangleF RelativeRectangle { get; private set; }

        public void OverrideChildMatrix(Transform3D? matrix)
        {
            if (matrix.HasValue)
            {
                _childFlags[ChildMatrixOverride] = true;
                ChildWorldMatrix = matrix.Value;
            }
            else
            {
                _childFlags[ChildMatrixOverride] = false;
            }

            LocalMatrixBecameDirty();
            WorldMatrixBecameDirty();
        }

        public void OverrideSelfMatrix(Transform3D? matrix)
        {
            if (matrix.HasValue)
            {
                _selfFlags[SelfMatrixOverride] = true;
                SelfWorldMatrix = matrix.Value;
            }
            else
            {
                _selfFlags[SelfMatrixOverride] = false;
            }

            LocalMatrixBecameDirty();
            WorldMatrixBecameDirty();
        }

        protected virtual void RecalculatePosition()
        {
            var parent = (DrawableTransform)Parent;
            // Make it relative to the parent.
            var width = _size.X.Value +
                        (parent?.RelativeRectangle.Width ?? WindowManager.VirtualScreen.X) * _size.X.Scale;
            var height = _size.Y.Value +
                         (parent?.RelativeRectangle.Height ?? WindowManager.VirtualScreen.Y) * _size.Y.Scale;
            var x = _position.X.Value;
            var y = _position.Y.Value;

            // Make it relative to the screen size.
            RelativeRectangle = GraphicsHelper.AlignRect(_alignment, new RectangleF(x, y, width, height),
                parent?.RelativeRectangle ?? WindowManager.Rectangle, true);

            sourceVertices[1].X = sourceVertices[3].X = width;
            sourceVertices[2].Y = sourceVertices[3].Y = height;

            Pivot = _pivot;
            CalculatedPosition = new Vector3(RelativeRectangle.Position, _calculatedPosition.Z);

            SelfWorldMatrix.Decompose(out var worldScale, out _, out var worldPos);
            var worldSize = new Vector2(RelativeRectangle.Width * worldScale.X,
                RelativeRectangle.Height * worldScale.Y);
            ScreenRectangle = new RectangleF(worldPos.X, worldPos.Y, worldSize.X, worldSize.Y);

            RecalculateVertices();

            PositionRecalculated?.Invoke();
        }

        private void RecalculateVertices()
        {
            SelfWorldMatrix.Transform(sourceVertices, Vertices);
            ScreenMinimumBoundingRectangle = GraphicsHelper.MinimumBoundingRectangle(
                Vertices[0], Vertices[1], Vertices[2], Vertices[3]);
        }

        private void OnParentChanged(WobbleBaseTransform<Transform3D> oldParent,
            WobbleBaseTransform<Transform3D> newParent)
        {
            if (oldParent is DrawableTransform oldDrawableTransform)
            {
                oldDrawableTransform.PositionRecalculated -= RecalculatePosition;
            }

            if (newParent is DrawableTransform newDrawableTransform)
            {
                newDrawableTransform.PositionRecalculated += RecalculatePosition;
            }

            RecalculatePosition();
        }

        protected override void RecalculateWorldMatrix(ref Transform3D localMatrix,
            out Transform3D worldMatrix)
        {
            if (Parent != null)
            {
                var parentChildWorldMatrix = Parent.ChildWorldMatrix;
                Transform3D.Multiply(ref localMatrix, ref parentChildWorldMatrix, out worldMatrix);
                if (_selfFlags.Data != 0 && !_selfFlags[SelfMatrixOverride])
                    Transform3D.Multiply(ref _selfLocalMatrix, ref parentChildWorldMatrix, out _selfWorldMatrix);
                if (_childFlags.Data != 0 && !_childFlags[ChildMatrixOverride])
                    Transform3D.Multiply(ref _childLocalMatrix, ref parentChildWorldMatrix, out _childWorldMatrix);
            }
            else
            {
                worldMatrix = localMatrix;
                if (NeedsSelfTransformCalculation)
                    _selfWorldMatrix = _selfLocalMatrix;
                if (NeedsChildTransformCalculation)
                    _childWorldMatrix = _childLocalMatrix;
            }
        }

        protected override void RecalculateLocalMatrix(out Transform3D localMatrix)
        {
            var rotation = Rotation;
            if (IndependentRotation && Parent != null)
            {
                var parentChildMatrix = Parent.ChildWorldMatrix;
                parentChildMatrix.Decompose(out _, out var parentWorldRotation, out _);
                rotation = Quaternion.Inverse(parentWorldRotation) * rotation;
            }

            localMatrix = new Transform3D(_calculatedPosition, rotation, new Vector3(_scale.X, _scale.Y, 1), _origin);

            if (NeedsSelfTransformCalculation)
            {
                var selfRotation = SelfRotation * rotation;
                _selfLocalMatrix = new Transform3D(_calculatedPosition, selfRotation,
                    new Vector3(_scale.X, _scale.Y, 1),
                    _origin);
            }

            if (NeedsChildTransformCalculation)
            {
                // ...
                _childLocalMatrix = localMatrix;
            }
        }

        public override string ToString()
        {
            return $"Position: {CalculatedPosition}, Rotation: {Rotation}, Scale: {Scale}";
        }
    }
}