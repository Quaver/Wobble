using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Wobble.Graphics
{
    [Flags]
    internal enum TransformFlags : byte
    {
        WorldMatrixIsDirty = 1 << 0,
        LocalMatrixIsDirty = 1 << 1,
        All = WorldMatrixIsDirty | LocalMatrixIsDirty
    }

    /// <summary>
    ///     Represents the base class for the position, rotation, and scale of a game object in two-dimensions or
    ///     three-dimensions.
    /// </summary>
    /// <typeparam name="TMatrix">The type of the matrix.</typeparam>
    /// <remarks>
    ///     <para>
    ///         Every game object has a transform which is used to store and manipulate the position, rotation and scale
    ///         of the object. Every transform can have a parent, which allows to apply position, rotation and scale to game
    ///         objects hierarchically.
    ///     </para>
    ///     <para>
    ///         This class shouldn't be used directly. Instead use either of the derived classes; <see cref="Transform2" /> or
    ///         Transform3D.
    ///     </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class WobbleBaseTransform<TMatrix>
        where TMatrix : struct
    {
        private TransformFlags _flags = TransformFlags.All; // dirty flags, set all dirty flags when created
        private TMatrix _selfLocalMatrix; // model space to local space
        private WobbleBaseTransform<TMatrix> _parent; // parent
        private TMatrix _selfWorldMatrix; // local space to world space
        private TMatrix _childWorldMatrix;
        private TMatrix _childLocalMatrix;

        // internal contructor because people should not be using this class directly; they should use Transform2D or Transform3D
        protected WobbleBaseTransform()
        {
        }

        /// <summary>
        ///     Gets the model-to-local space.
        /// </summary>
        /// <value>
        ///     The model-to-local space.
        /// </value>
        public TMatrix SelfLocalMatrix
        {
            get
            {
                RecalculateLocalMatrixIfNecessary(); // attempt to update local matrix upon request if it is dirty
                return _selfLocalMatrix;
            }
        }

        /// <summary>
        ///     Gets the local-to-world space.
        /// </summary>
        /// <value>
        ///     The local-to-world space.
        /// </value>
        public TMatrix SelfWorldMatrix
        {
            get
            {
                RecalculateWorldMatrixIfNecessary(); // attempt to update world matrix upon request if it is dirty
                return _selfWorldMatrix;
            }
        }

        /// <summary>
        ///     Gets the local-to-world space for children.
        /// </summary>
        /// <value>
        ///     The local-to-world space for children.
        /// </value>
        public TMatrix ChildWorldMatrix
        {
            get
            {
                RecalculateWorldMatrixIfNecessary(); // attempt to update world matrix upon request if it is dirty
                return _childWorldMatrix;
            }
        }

        /// <summary>
        ///     Gets the local-to-world space for children.
        /// </summary>
        /// <value>
        ///     The local-to-world space for children.
        /// </value>
        public TMatrix ChildLocalMatrix
        {
            get
            {
                RecalculateLocalMatrixIfNecessary(); // attempt to update world matrix upon request if it is dirty
                return _childLocalMatrix;
            }
        }

        /// <summary>
        ///     Gets or sets the parent instance.
        /// </summary>
        /// <value>
        ///     The parent instance.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Setting <see cref="Parent" /> to a non-null instance enables this instance to
        ///         inherit the position, rotation, and scale of the parent instance. Setting <see cref="Parent" /> to
        ///         <code>null</code> disables the inheritance altogether for this instance.
        ///     </para>
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public WobbleBaseTransform<TMatrix> Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                    return;

                var oldParentTransform = Parent;
                _parent = value;
                OnParentChanged(oldParentTransform, value);
            }
        }

        public event Action TransformBecameDirty; // observer pattern for when the world (or local) matrix became dirty
        public event Action TranformUpdated; // observer pattern for after the world (or local) matrix was re-calculated

        protected internal void LocalMatrixBecameDirty()
        {
            _flags |= TransformFlags.LocalMatrixIsDirty;
        }

        protected internal void WorldMatrixBecameDirty()
        {
            _flags |= TransformFlags.WorldMatrixIsDirty;
            TransformBecameDirty?.Invoke();
        }

        private void OnParentChanged(WobbleBaseTransform<TMatrix> oldParent, WobbleBaseTransform<TMatrix> newParent)
        {
            var parent = oldParent;
            while (parent != null)
            {
                parent.TransformBecameDirty -= ParentOnTransformBecameDirty;
                parent = parent.Parent;
            }

            parent = newParent;
            while (parent != null)
            {
                parent.TransformBecameDirty += ParentOnTransformBecameDirty;
                parent = parent.Parent;
            }
        }

        private void ParentOnTransformBecameDirty()
        {
            _flags |= TransformFlags.All;
        }

        private void RecalculateWorldMatrixIfNecessary()
        {
            if ((_flags & TransformFlags.WorldMatrixIsDirty) == 0)
                return;

            RecalculateLocalMatrixIfNecessary();
            RecalculateWorldMatrix(ref _selfLocalMatrix, ref _childLocalMatrix, out _selfWorldMatrix,
                out _childWorldMatrix);

            _flags &= ~TransformFlags.WorldMatrixIsDirty;
            TranformUpdated?.Invoke();
        }

        protected abstract void RecalculateWorldMatrix(ref TMatrix selfLocalMatrix, ref TMatrix childLocalMatrix,
            out TMatrix selfWorldMatrix,
            out TMatrix childWorldMatrix);

        private void RecalculateLocalMatrixIfNecessary()
        {
            if ((_flags & TransformFlags.LocalMatrixIsDirty) == 0)
                return;

            RecalculateLocalMatrix(out _selfLocalMatrix, out _childLocalMatrix);

            _flags &= ~TransformFlags.LocalMatrixIsDirty;
            WorldMatrixBecameDirty();
        }

        protected abstract void RecalculateLocalMatrix(out TMatrix selfLocalMatrix, out TMatrix childLocalMatrix);
    }

    /// <summary>
    ///     Represents the position, rotation, and scale of a three-dimensional game object.
    /// </summary>
    /// <seealso cref="BaseTransform{Matrix}" />
    /// <remarks>
    ///     <para>
    ///         Every game object has a transform which is used to store and manipulate the position, rotation and scale
    ///         of the object. Every transform can have a parent, which allows to apply position, rotation and scale to game
    ///         objects hierarchically.
    ///     </para>
    /// </remarks>
    public class QuadTransform : WobbleBaseTransform<Matrix>
    {
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale = Vector3.One;
        private Vector3 _origin = Vector3.Zero;
        private bool _independentRotation;

        public QuadTransform(Vector3? position = null, Quaternion? rotation = null, Vector2? scale = null,
            Vector3? origin = null)
        {
            Position = position ?? Vector3.Zero;
            Rotation = rotation ?? Quaternion.Identity;
            Scale = new Vector3(scale ?? Vector2.One, 0);
            Origin = origin ?? Vector3.Zero;
        }

        /// <summary>
        ///     Gets the world position.
        /// </summary>
        /// <value>
        ///     The world position.
        /// </value>
        public Vector3 WorldPosition => SelfWorldMatrix.Translation;

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
                SelfWorldMatrix.Decompose(out var scale, out _, out _);
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
                SelfWorldMatrix.Decompose(out _, out var rotation, out _);
                return rotation;
            }
        }

        /// <summary>
        ///     Gets or sets the local position.
        /// </summary>
        /// <value>
        ///     The local position.
        /// </value>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
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
            get { return _rotation; }
            set
            {
                _rotation = value;
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
        public Vector3 Scale
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
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public Plane Plane
        {
            get
            {
                var worldMatrix = SelfWorldMatrix;
                return new Plane(worldMatrix.Translation, Vector3.Cross(worldMatrix.Right, worldMatrix.Up));
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

        protected override void RecalculateWorldMatrix(ref Matrix selfLocalMatrix, ref Matrix childLocalMatrix,
            out Matrix selfWorldMatrix,
            out Matrix childWorldMatrix)
        {
            if (Parent != null)
            {
                var parentChildWorldMatrix = Parent.ChildWorldMatrix;
                Matrix.Multiply(ref selfLocalMatrix, ref parentChildWorldMatrix, out selfWorldMatrix);
            }
            else
            {
                selfWorldMatrix = selfLocalMatrix;
            }

            childWorldMatrix = selfWorldMatrix;
        }

        protected override void RecalculateLocalMatrix(out Matrix selfLocalMatrix, out Matrix childLocalMatrix)
        {
            var originMatrix = Matrix.CreateTranslation(-Origin);
            var scaleMatrix = Matrix.CreateScale(_scale);
            var rotation = _rotation;
            if (IndependentRotation && Parent != null)
            {
                var parentChildMatrix = Parent.ChildWorldMatrix;
                parentChildMatrix.Decompose(out _, out var parentWorldRotation, out _);
                rotation = Quaternion.Inverse(parentWorldRotation) * rotation;
            }
            var rotationMatrix = Matrix.CreateFromQuaternion(rotation);
            var positionMatrix = Matrix.CreateTranslation(Origin + _position);
            Matrix.Multiply(ref originMatrix, ref scaleMatrix, out var m1);
            Matrix.Multiply(ref rotationMatrix, ref positionMatrix, out var m2);
            Matrix.Multiply(ref m1, ref m2, out childLocalMatrix);

            selfLocalMatrix = childLocalMatrix;
        }

        public override string ToString()
        {
            return $"Position: {Position}, Rotation: {Rotation}, Scale: {Scale}";
        }
    }

    public class IndependentQuadTransform : QuadTransform
    {
        protected override void RecalculateWorldMatrix(ref Matrix selfLocalMatrix, ref Matrix childLocalMatrix,
            out Matrix selfWorldMatrix, out Matrix childWorldMatrix)
        {
            selfWorldMatrix = selfLocalMatrix;
            childWorldMatrix = childLocalMatrix;
        }
    }
}