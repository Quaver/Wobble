using System;

namespace Wobble.Configuration
{
    /// <summary>
    ///     Requires a property to be explicitly present in a YAML main document. Invalid required properties
    ///     reject the complete main document instead of falling back at property level.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigRequiredAttribute : Attribute
    {
    }
}
