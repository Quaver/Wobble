using System;

namespace Wobble.Graphics
{
    [Flags]
    public enum LayerFlags
    {
        None = 0,

        /// <summary>
        ///     No layers can be put on top of this layer
        /// </summary>
        Top = 1 << 0,

        /// <summary>
        ///     No layers can be put below this layer
        /// </summary>
        Bottom = 1 << 1,

        /// <summary>
        ///     No children can be attached to this layer
        /// </summary>
        NoChildren = 1 << 2,
    }
}