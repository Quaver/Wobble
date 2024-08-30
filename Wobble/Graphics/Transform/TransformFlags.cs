using System;

namespace Wobble.Graphics.Transform
{
    [Flags]
    internal enum TransformFlags : byte
    {
        WorldMatrixIsDirty = 1 << 0,
        LocalMatrixIsDirty = 1 << 1,
        All = WorldMatrixIsDirty | LocalMatrixIsDirty
    }
}