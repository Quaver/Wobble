using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Wobble.Bindables
{
    public class BindableFloat : Bindable<float>
    {
        /// <summary>
        ///     The mininimum value that it will be clamped to.
        /// </summary>
        public float MinValue { get; }

        /// <summary>
        ///     The maximum value that it will be clamped to
        /// </summary>
        public float MaxValue { get; }

        /// <summary>
        ///     The value of the BindableFloat
        /// </summary>
        private float _value;
        public new float Value
        {
            get => _value;
            set
            {
                var previousVal = _value;
                _value = MathHelper.Clamp(value, MinValue, MaxValue);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_value != previousVal)
                    ValueChanged?.Invoke(this, new BindableValueChangedEventArgs<float>(_value, previousVal));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="action"></param>
        public BindableFloat(float defaultVal, float min, float max, EventHandler<BindableValueChangedEventArgs<float>> action = null)
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
        public BindableFloat(string name, float defaultVal, float min, float max, EventHandler<BindableValueChangedEventArgs<float>> action = null)
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