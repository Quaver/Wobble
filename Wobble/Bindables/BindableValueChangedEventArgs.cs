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
        /// <param name="value">The new value of the bindable</param>
        /// <param name="oldValue">The old value of the bindable</param>
        public BindableValueChangedEventArgs(T value, T oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }

        /// <summary>
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        ///     The old value.
        /// </summary>
        public T OldValue { get; set; }
    }
}
