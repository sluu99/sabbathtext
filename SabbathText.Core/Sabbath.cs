using SabbathText.Core.Entities;
using System;

namespace SabbathText.Core
{
    public class Sabbath
    {
        /// <remarks>If this is called on the Sabbath day, it'll return up to 10 minutes of margin</remarks>
        /// <returns></returns>
        public static DateTime GetLocationNextSabbath(double latitude, double longitude, double timeZoneOffset)
        {
            return GetLocationNextSabbath(latitude, longitude, timeZoneOffset, TimeSpan.FromMinutes(10));
        }

        public static DateTime GetLocationNextSabbath(double latitude, double longitude, double timeZoneOffset, TimeSpan sabbathDayMargin)
        {
            // test location
            if (latitude == Location.TestLocation.Latitude && longitude == Location.TestLocation.Longitude)
            {
                return Clock.UtcNow.AddMinutes(2);
            }

            DateTime destinationTime = Clock.UtcNow.AddHours(timeZoneOffset);

            int daysUntilFriday = DaysUntilFriday(destinationTime.Date);

            DateTime utcSunsetTime = Clock.UtcNow;
            DateTime destinationSunsetTime = Clock.UtcNow;

            if (daysUntilFriday == 0)
            {
                // today is Friday, check if the Sun already set                
                TryGetUtcSunSetTime(destinationTime.Date, latitude, longitude, out utcSunsetTime);
                destinationSunsetTime = utcSunsetTime.AddHours(timeZoneOffset);
                destinationSunsetTime = new DateTime(destinationTime.Year, destinationTime.Month, destinationTime.Day, destinationSunsetTime.Hour, destinationSunsetTime.Minute, destinationSunsetTime.Second);

                // Sun has not set yet, Sabbath is today!
                if ((destinationSunsetTime + sabbathDayMargin) > destinationTime)
                {
                    return destinationSunsetTime + sabbathDayMargin;
                }
                else
                {
                    daysUntilFriday = 7; // wait until next week
                }
            }

            // go to next Friday
            DateTime destinationNextFriday = destinationTime.AddDays(daysUntilFriday);

            TryGetUtcSunSetTime(destinationNextFriday.Date, latitude, longitude, out utcSunsetTime);
            destinationSunsetTime = utcSunsetTime.AddHours(timeZoneOffset);

            return new DateTime(destinationNextFriday.Year, destinationNextFriday.Month, destinationNextFriday.Day, destinationSunsetTime.Hour, destinationSunsetTime.Minute, destinationSunsetTime.Second);
        }

        private static int DaysUntilFriday(DateTime date)
        {
            int offset = DayOfWeek.Friday - date.DayOfWeek;

            if (offset < 0) // next week
            {
                offset += 7;
            }

            return offset;
        }

        private static bool TryGetUtcSunSetTime(DateTime date, double latitude, double longitude, out DateTime sunSetTimeUtc)
        {
            bool isSunrise = false;
            bool isSunset = false;
            DateTime sunRise = Clock.UtcNow;
            DateTime sunSet = Clock.UtcNow;

            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);

            sunSetTimeUtc = Clock.UtcNow;

            if (!SunTimes.Instance.CalculateSunRiseSetTimes(latitude, longitude, date, ref sunRise, ref sunSet, ref isSunrise, ref isSunset))
            {
                return false;
            }

            if (!isSunset)
            {
                return false;
            }
            
            // sunSet now has the sunset time, in the local machine's time zone
            sunSetTimeUtc = sunSet.ToUniversalTime();
            
            return true;
        }
    }
}
