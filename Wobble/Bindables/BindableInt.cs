using System;
using Microsoft.Xna.Framework;

namespace Wobble.Bindables
{
    /// <inheritdoc />
    /// <summary>
    ///     Bindable Int32 value. Contains extra stuff such as Max/Min values.
    /// </summary>
    internal class BindableInt : Bindable<int>
    {
        /// <summary>
        ///     The mininimum value that it will be clamped to.
        /// </summary>
        internal int MinValue { get; }

        /// <summary>
        ///     The maximum value that it will be clamped to
        /// </summary>
        internal int MaxValue { get; }

        /// <summary>
        ///     The value of this BindedInt
        /// </summary>
        private int _value;
        internal new int Value
        {
            get => _value;
            set
            {
                var previousVal = _value;
                _value = MathHelper.Clamp(value, MinValue, MaxValue);

                if (_value != previousVal)
                    ValueChanged?.Invoke(this, new BindableValueChangedEventArgs<int>(_value, previousVal));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="action"></param>
        public BindableInt(int defaultVal, int min, int max, EventHandler<BindableValueChangedEventArgs<int>> action = null)
            : base(defaultVal, action)
        {
            MinValue = min;
            MaxValue = max;
            Value = defaultVal;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="action"></param>
        public BindableInt(string name, int defaultVal, int min, int max, EventHandler<BindableValueChangedEventArgs<int>> action = null)
            : base(name, defaultVal, action)
        {
            MinValue = min;
            MaxValue = max;
            Value = defaultVal;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Prints a stringified value of the Bindable.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value.ToString();
    }
}