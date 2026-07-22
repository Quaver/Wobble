using System;

namespace Wobble.Configuration
{
    /// <summary>
    ///     Allows a configuration property, including its nested properties, to be changed by player overrides
    ///     or the public mutation API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigEditableAttribute : Attribute
    {
    }
}
