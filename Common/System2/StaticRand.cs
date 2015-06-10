namespace System
{
    /// <summary>
    /// A static random class
    /// </summary>
    public static class StaticRand
    {
        private static readonly Random Rand = new Random(Environment.TickCount);

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than <see cref="System.Int32.MaxValue"/></returns>
        public static int Next()
        {
            return Rand.Next();
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///   The exclusive upper bound of the random number to be generated. maxValue
        ///   must be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than maxValue;
        ///     that is, the range of return values ordinarily includes zero but not maxValue.
        ///     However, if maxValue equals zero, maxValue is returned.
        /// </returns>
        public static int Next(int maxValue)
        {
            return Rand.Next(maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///   The exclusive upper bound of the random number returned. maxValue must be
        ///   greater than or equal to minValue.</param>
        /// <returns>
        ///   A 32-bit signed integer greater than or equal to minValue and less than maxValue;
        ///   that is, the range of return values includes minValue but not maxValue. If
        ///   minValue equals maxValue, minValue is returned.
        /// </returns>
        public static int Next(int minValue, int maxValue)
        {
            return Rand.Next(minValue, maxValue);
        }
    }
}
