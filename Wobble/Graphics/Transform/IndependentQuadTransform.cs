using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Transform
{
    public class IndependentQuadTransform : QuadTransform
    {
        protected override void RecalculateWorldMatrix(ref Matrix localMatrix,
            out Matrix worldMatrix)
        {
            worldMatrix = localMatrix;
        }
    }
}