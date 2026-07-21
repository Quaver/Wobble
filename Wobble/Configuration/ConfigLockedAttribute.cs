using System;

namespace Wobble.Configuration
{
    /// <summary>
    ///     Prevents a configuration property, including its nested properties, from being changed by player
    ///     overrides or the public mutation API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigLockedAttribute : Attribute
    {
    }
}
