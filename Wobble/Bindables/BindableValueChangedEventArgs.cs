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
    public class BindableValueChangedEventArgs<T> : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="value"></param>
        public BindableValueChangedEventArgs(T value, T oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }

        /// <summary>
        ///     The value passed when
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        ///     The old value.
        /// </summary>
        public T OldValue { get; set; }
    }
}
