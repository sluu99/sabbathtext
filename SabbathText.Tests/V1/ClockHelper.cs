namespace SabbathText.Tests.V1
{
    using System;
    using System.Threading;
    using SabbathText.Location.V1;

    /// <summary>
    /// Helper class for the clock
    /// </summary>
    public static class ClockHelper
    {
        /// <summary>
        /// Gets the <see cref="DateTime"/> for the next Sabbath at a specific location.
        /// </summary>
        /// <param name="zipCode">The ZIP code of the location.</param>
        /// <param name="sabbathEndTime">The Sabbath end time, in UTC</param>
        /// <returns>The time of the next Sabbath in UTC.</returns>
        public static DateTime GetSabbathStartTime(string zipCode, out DateTime sabbathEndTime)
        {
            LocationInfo location = LocationInfo.FromZipCode(zipCode);
            
            DayOfWeek today = location.LocalTime.DayOfWeek;
            if (today == DayOfWeek.Friday)
            {
                TimeInfo timeInfo = TimeInfo.Create(zipCode, location.LocalTime.Date);
                if (Clock.UtcNow < timeInfo.SunSetUtc)
                {
                    // sunset of the next day
                    sabbathEndTime = TimeInfo.Create(zipCode, timeInfo.SunSetDestination.Date.AddDays(1)).SunSetUtc;
                    return timeInfo.SunSetUtc;
                }
            }

            // go to next Friday
            int dayDelta = DayOfWeek.Friday - today;
            if (dayDelta < 1)
            {
                dayDelta = 7 + dayDelta;
            }

            TimeInfo sabbathTimeInfo = TimeInfo.Create(zipCode, location.LocalTime.Date.AddDays(dayDelta));

            // sunset of the next day
            sabbathEndTime = TimeInfo.Create(zipCode, sabbathTimeInfo.SunSetDestination.Date.AddDays(1)).SunSetUtc;
            return sabbathTimeInfo.SunSetUtc;            
        }

        /// <summary>
        /// Goes to a specific date time.
        /// </summary>
        /// <param name="dateTime">The date time to go to.</param>
        public static void GoTo(DateTime dateTime)
        {
            Clock.Delay(dateTime - Clock.UtcNow).Wait();
        }

        /// <summary>
        /// Roll the clock to some time before noon of the specified day of the week.
        /// </summary>
        /// <param name="dayOfWeek">The day of the week.</param>
        public static void GoToDay(DayOfWeek dayOfWeek)
        {
            // make sure we are before noon
            int hoursForward = 0;
            if (Clock.UtcNow.Hour > 12)
            {
                // go to the next day
                hoursForward = 25 - Clock.UtcNow.Hour;
            }

            Clock.Delay(TimeSpan.FromHours(hoursForward)).Wait();

            // go to the right day
            int dayDelta = dayOfWeek - Clock.UtcNow.DayOfWeek;
            if (dayDelta < 0)
            {
                // go to next week
                dayDelta = 7 + dayDelta;
            }

            Clock.Delay(TimeSpan.FromDays(dayDelta)).Wait();
        }
    }
}
