using System;
using System.ComponentModel;
using MonoGame.Extended;

namespace Wobble.Graphics.Transform
{
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
        private TMatrix _localMatrix; // model space to local space
        private WobbleBaseTransform<TMatrix> _parent; // parent
        private TMatrix _worldMatrix; // local space to world space
        private WobbleBaseTransform<TMatrix> _childTransform;
        private WobbleBaseTransform<TMatrix> _selfTransform;

        // TODO
        public WobbleBaseTransform<TMatrix> ChildTransform
        {
            get => _childTransform ?? this;
            set
            {
                _childTransform = value;
                _childTransform.Parent = this;
            }
        }

        public WobbleBaseTransform<TMatrix> SelfTransform
        {
            get => _selfTransform ?? this;
            set
            {
                _selfTransform = value;
                _childTransform.Parent = this;
            }
        }

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
        public TMatrix LocalMatrix
        {
            get
            {
                RecalculateLocalMatrixIfNecessary(); // attempt to update local matrix upon request if it is dirty
                return _localMatrix;
            }
        }

        /// <summary>
        ///     Gets the local-to-world space.
        /// </summary>
        /// <value>
        ///     The local-to-world space.
        /// </value>
        public TMatrix WorldMatrix
        {
            get
            {
                RecalculateWorldMatrixIfNecessary(); // attempt to update world matrix upon request if it is dirty
                return _worldMatrix;
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
            RecalculateWorldMatrix(ref _localMatrix, out _worldMatrix);

            _flags &= ~TransformFlags.WorldMatrixIsDirty;
            TranformUpdated?.Invoke();
        }

        protected abstract void RecalculateWorldMatrix(ref TMatrix localMatrix, out TMatrix worldMatrix);

        private void RecalculateLocalMatrixIfNecessary()
        {
            if ((_flags & TransformFlags.LocalMatrixIsDirty) == 0)
                return;

            RecalculateLocalMatrix(out _localMatrix);

            _flags &= ~TransformFlags.LocalMatrixIsDirty;
            WorldMatrixBecameDirty();
        }

        protected abstract void RecalculateLocalMatrix(out TMatrix localMatrix);
    }
}