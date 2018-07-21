using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wobble.Bindables
{
    /// <inheritdoc />
    /// <summary>
    ///     EventArgs containing the value that was changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BindableValueChangedEventArgs<T> : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="value"></param>
        internal BindableValueChangedEventArgs(T value, T oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }

        /// <summary>
        ///     The value passed when
        /// </summary>
        internal T Value { get; set; }

        /// <summary>
        ///     The old value.
        /// </summary>
        internal T OldValue { get; set; }
    }
}
