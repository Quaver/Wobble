using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Transform;

public static class MatrixHelper
{
    /// <summary>
    ///     Result of <see cref="Matrix.CreateTranslation(Vector3)"/> * <see cref="Matrix.CreateScale(Vector3)"/>
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Matrix CreateTranslationScale(Vector3 translation, Vector3 scale)
    {
        return new Matrix(
            scale.X, 0, 0, 0,
            0, scale.Y, 0, 0,
            0, 0, scale.Z, 0,
            translation.X * scale.X, translation.Y * scale.Y, translation.Z * scale.Z, 1
        );
    }
}