namespace Wobble.Graphics
{
    public struct ScalableVector
    {
        /// <summary>
        ///     The value of the vector.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        ///     The scale of the vector.
        /// </summary>
        public float Scale { get; set; }

        public ScalableVector(float value = 0, float scale = 0)
        {
            Value = value;
            Scale = scale;
        }

        public override string ToString() => $"(Value: {Value}, Scale: {Scale})";
    }
}