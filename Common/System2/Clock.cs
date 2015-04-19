namespace System
{
    using System.Threading;

    /// <summary>
    /// This class provides a way to get time, which allows test cases to manipulate the time
    /// </summary>
    public class Clock
    {
        /// <summary>
        /// The minimum value for DateTime
        /// </summary>
        public static readonly DateTime MinValue = DateTime.MinValue;

        /// <summary>
        /// The maximum value for DateTime
        /// </summary>
        public static readonly DateTime MaxValue = DateTime.MaxValue;

        /// <summary>
        /// Indicates whether fake clock is in use
        /// </summary>
        private static bool fakeClock = false;

        /// <summary>
        /// The amount of time offset from the real time
        /// </summary>
        private static TimeSpan fakeClockOffset = TimeSpan.Zero;

        /// <summary>
        /// Indicates whether the clock can change modes
        /// </summary>
        private static bool frozen = false;

        /// <summary>
        /// Gets the current UTC date time
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (fakeClock)
                {
                    return DateTime.UtcNow + fakeClockOffset;
                }

                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Sleep for an amount of time.
        /// </summary>
        /// <param name="timeout">The amount of time to sleep.</param>
        public static void Sleep(TimeSpan timeout)
        {
            if (fakeClock)
            {
                RollClock(timeout);
            }
            else
            {
                Thread.Sleep(timeout);
            }
        }

        /// <summary>
        /// Set the clock to use the real system time
        /// </summary>
        public static void UseSystemClock()
        {
            if (frozen)
            {
                throw new ApplicationException("Cannot change clock mode when it is frozen");
            }

            fakeClock = false;
        }

        /// <summary>
        /// Switches to use the fake clock
        /// </summary>
        public static void UseFakeClock()
        {
            if (frozen)
            {
                throw new ApplicationException("Cannot change clock mode when it is frozen");
            }

            fakeClock = true;
        }

        /// <summary>
        /// Freezes the clock, which prevents mode switching between system clock and fake clock
        /// </summary>
        public static void Freeze()
        {
            frozen = true;
        }

        /// <summary>
        /// Move the clock a certain amount of time
        /// </summary>
        /// <param name="offset">The offset to be added</param>
        public static void RollClock(TimeSpan offset)
        {
            fakeClockOffset += offset;
        }

        /// <summary>
        /// Resets the clock to the current time
        /// </summary>
        public static void ResetClock()
        {
            fakeClockOffset = TimeSpan.Zero;
        }
    }
}