namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a range of floating-point values.
    /// </summary>
    /// <author>Stuart Brown</author>
    public class FloatRange
    {
        public float MinValue { get; private set; }

        public float MaxValue { get; private set; }

        /// <summary>
        /// Gets the size of the range (i.e. the difference between the maximum and minimum values).
        /// </summary>
        public float RangeSize => MaxValue - MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatRange"/> class.
        /// </summary>
        public FloatRange()
        {
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
        }

        /// <summary>
        /// Adds a value to the range.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void AddValue(float value)
        {
            if (value < MinValue) MinValue = value;
            if (value > MaxValue) MaxValue = value;
        }
    }
}