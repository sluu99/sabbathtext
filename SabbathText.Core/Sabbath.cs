using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class Sabbath
    {
        public static DateTime GetLocationNextSabbath(double latitude, double longitude, double timeZoneOffset)
        {
            DateTime destinationTime = Clock.UtcNow.AddHours(timeZoneOffset);
            Trace.TraceInformation("Destination time: {0}", destinationTime);

            int daysUntilFriday = DaysUntilFriday(destinationTime.Date);
            Trace.TraceInformation("Days until Friday: {0}", daysUntilFriday);

            DateTime utcSunsetTime = Clock.UtcNow;
            DateTime destinationSunsetTime = Clock.UtcNow;

            if (daysUntilFriday == 0)
            {
                // today is Friday, check if the Sun already set                
                TryGetUtcSunSetTime(destinationTime.Date, latitude, longitude, out utcSunsetTime);
                destinationSunsetTime = utcSunsetTime.AddHours(timeZoneOffset);

                // Sun has not set yet, Sabbath is today!
                if (destinationSunsetTime > destinationTime)
                {
                    return destinationSunsetTime;
                }
                else
                {
                    daysUntilFriday = 7; // wait until next week
                }
            }

            // go to next Friday
            DateTime destinationNextFriday = destinationTime.AddDays(daysUntilFriday);
            Trace.TraceInformation("Next Friday: {0}", destinationNextFriday);

            TryGetUtcSunSetTime(destinationNextFriday.Date, latitude, longitude, out utcSunsetTime);
            destinationSunsetTime = utcSunsetTime.AddHours(timeZoneOffset);

            return destinationSunsetTime;
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
            DateTime sunSetTimeLocal = new DateTime(sunSet.Ticks, DateTimeKind.Local);
            sunSetTimeUtc = sunSetTimeLocal.ToUniversalTime();

            Trace.TraceInformation("Sunset: {0}", sunSet);
            Trace.TraceInformation("Sunset local: {0}", sunSetTimeLocal);
            Trace.TraceInformation("Sunset UTC: {0}", sunSetTimeUtc);

            return true;
        }
    }
}
