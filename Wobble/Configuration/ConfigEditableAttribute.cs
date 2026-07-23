using System;

namespace Wobble.Configuration
{
    /// <summary>
    ///     Allows a configuration property, including its nested properties, to be changed by external YAML or
    ///     the public mutation API when the configuration is loaded in restricted mode.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigEditableAttribute : Attribute
    {
    }
}
