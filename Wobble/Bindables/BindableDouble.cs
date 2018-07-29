using System;
using System.Globalization;

namespace Wobble.Bindables
{
    /// <inheritdoc />
    /// <summary>
    ///     Bindable of type double. Includes extras such as min/max value.
    /// </summary>
    public class BindableDouble : Bindable<double>
    {
        /// <summary>
        ///     The mininimum value that it will be clamped to.
        /// </summary>
        public double MinValue { get; }

        /// <summary>
        ///     The maximum value that it will be clamped to
        /// </summary>
        public double MaxValue { get; }

        /// <summary>
        ///     The value of the BindableDouble
        /// </summary>
        private double _value;
        public new double Value
        {
            get => _value;
            set
            {
                var previousVal = _value;

                // Manually clamp here since MathHelper.Clamp doesn't support it.
                if (value <= MinValue)
                    _value = MinValue;
                else if (value >= MaxValue)
                    _value = MaxValue;
                else
                    _value = value;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_value != previousVal)
                    ValueChanged?.Invoke(this, new BindableValueChangedEventArgs<double>(_value, previousVal));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="action"></param>
        public BindableDouble(double defaultVal, double min, double max, EventHandler<BindableValueChangedEventArgs<double>> action = null)
            : base(defaultVal, action)
        {
            MinValue = min;
            MaxValue = max;
            Value = defaultVal;
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="action"></param>
        public BindableDouble(string name, double defaultVal, double min, double max, EventHandler<BindableValueChangedEventArgs<double>> action = null)
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
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }
}