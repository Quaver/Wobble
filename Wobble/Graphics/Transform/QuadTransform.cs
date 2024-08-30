using Microsoft.Xna.Framework;
using MonoGame.Extended;

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
    public class QuadTransform : WobbleBaseTransform<Matrix>
    {
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector2 _scale = Vector2.One;
        private Vector3 _origin = Vector3.Zero;
        private bool _independentRotation;

        public QuadTransform(Vector3? position = null, Quaternion? rotation = null, Vector2? scale = null,
            Vector3? origin = null)
        {
            Position = position ?? Vector3.Zero;
            Rotation = rotation ?? Quaternion.Identity;
            Scale = scale ?? Vector2.One;
            Origin = origin ?? Vector3.Zero;
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
                WorldMatrix.Decompose(out var scale, out _, out _);
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
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public Plane Plane
        {
            get
            {
                var worldMatrix = WorldMatrix;
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

        protected override void RecalculateWorldMatrix(ref Matrix localMatrix,
            out Matrix worldMatrix)
        {
            if (Parent != null)
            {
                var parentChildWorldMatrix = Parent.WorldMatrix;
                Matrix.Multiply(ref localMatrix, ref parentChildWorldMatrix, out worldMatrix);
            }
            else
            {
                worldMatrix = localMatrix;
            }
        }

        protected override void RecalculateLocalMatrix(out Matrix localMatrix)
        {
            var originMatrix = Matrix.CreateTranslation(-Origin);
            var scaleMatrix = Matrix.CreateScale(_scale.X, _scale.Y, 1);
            var rotation = _rotation;
            if (IndependentRotation && Parent != null)
            {
                var parentChildMatrix = Parent.WorldMatrix;
                parentChildMatrix.Decompose(out _, out var parentWorldRotation, out _);
                rotation = Quaternion.Inverse(parentWorldRotation) * rotation;
            }
            var rotationMatrix = Matrix.CreateFromQuaternion(rotation);
            var positionMatrix = Matrix.CreateTranslation(Origin + _position);
            Matrix.Multiply(ref originMatrix, ref scaleMatrix, out var m1);
            Matrix.Multiply(ref rotationMatrix, ref positionMatrix, out var m2);
            Matrix.Multiply(ref m1, ref m2, out localMatrix);
        }

        public override string ToString()
        {
            return $"Position: {Position}, Rotation: {Rotation}, Scale: {Scale}";
        }
    }
}