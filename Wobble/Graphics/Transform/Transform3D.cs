using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Plane = Microsoft.Xna.Framework.Plane;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Wobble.Graphics.Transform
{
    public struct Transform3D
    {
        private Matrix _matrix = Matrix.Identity;

        public Matrix Matrix => _matrix;

        public Vector3 Translation
        {
            get => _matrix.Translation;
            set => _matrix.Translation = value;
        }

        public float RotationZ
        {
            get
            {
                Decompose2D(out _, out var rotation, out _);
                return rotation;
            }
        }

        public Plane Plane => new (_matrix.Translation, Vector3.Cross(_matrix.Right, _matrix.Up));

        public Transform3D()
        {
        }

        public Transform3D(Matrix matrix)
        {
            _matrix = matrix;
        }

        public Transform3D(Transform3D transform)
        {
            _matrix = transform._matrix;
        }

        public Transform3D(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var scaleMatrix = Matrix.CreateScale(scale);
            var rotationMatrix = Matrix.CreateFromQuaternion(rotation);
            Matrix.Multiply(ref scaleMatrix, ref rotationMatrix, out _matrix);
            Translate(position);
        }

        public Transform3D(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 origin)
        {
            var m1 = MatrixHelper.CreateTranslationScale(-origin, scale);
            var rotationMatrix = Matrix.CreateFromQuaternion(rotation);
            Matrix.Multiply(ref m1, ref rotationMatrix, out _matrix);
            Translate(origin + position);
        }

        public void Invert()
        {
            Matrix.Invert(ref _matrix, out _matrix);
        }

        public Transform3D Inverted()
        {
            var transform = new Transform3D(this);
            transform.Invert();
            return transform;
        }

        public Vector3 Transform(Vector3 vector)
        {
            return Vector3.Transform(vector, _matrix);
        }

        public void Transform(Vector3[] source, Vector3[] result)
        {
            Transform(source, ref this, result);
        }

        public static void Transform(Vector3[] source, ref Transform3D transform, Vector3[] result)
        {
            Vector3.Transform(source, ref transform._matrix, result);
        }

        public static Transform3D Transform(Transform3D left, Transform3D right)
        {
            Multiply(ref left, ref right, out var result);
            return result;
        }

        public Vector3 BasisTransform(Vector3 vector)
        {
            return Vector3.TransformNormal(vector, _matrix);
        }

        public void TranslateLocal(Vector3 translation)
        {
            _matrix.Translation += BasisTransform(translation);
        }

        public void Translate(Vector3 translation)
        {
            _matrix.Translation += translation;
        }

        public void Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position)
        {
            _matrix.Decompose(out scale, out rotation, out position);
        }

        public void Decompose2D(out Vector2 scale, out float rotation, out Vector2 position)
        {
            _matrix.Decompose(out position, out rotation, out scale);
        }

        public static void Multiply(ref Transform3D matrix, ref Transform3D transform, out Transform3D result)
        {
            result = new Transform3D();
            Matrix.Multiply(ref matrix._matrix, ref transform._matrix, out result._matrix);
        }
    }
}