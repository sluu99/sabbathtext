using NodaTime;
using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class Sabbath
    {
        public Sabbath()
        {
            this.DataProvider = new AzureDataProvider();
        }

        public IDataProvider DataProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="sunsetNotWithin">The sunset cannot be within this time span (used only if today is Friday)</param>
        /// <returns>The upcoming Sabbath time for a specific location. The time is in UTC</returns>
        public async Task<DateTime> GetUpcomingSabbath(Location location, TimeSpan sunsetNotWithin)
        {
            DateTimeZone destinationTimeZone = DateTimeZoneProviders.Tzdb[location.TimeZoneName];
            DateTime now = Clock.UtcNow;
            DateTime destinationNow = Instant.FromDateTimeUtc(now).InZone(destinationTimeZone).ToDateTimeUnspecified();
            
            LocationTimeInfo timeInfo = null;

            if (destinationNow.DayOfWeek == DayOfWeek.Friday)
            {
                timeInfo = await this.DataProvider.GetTimeInfoByZipCode(location.ZipCode, destinationNow.Date);

                if (timeInfo.Sunset + sunsetNotWithin < now) // the sun has not set yet
                {
                    return timeInfo.Sunset;
                }
            }
            
            DateTime destNextFriday = destinationNow.AddDays(DaysUntilNextFriday(destinationNow));
            timeInfo = await this.DataProvider.GetTimeInfoByZipCode(location.ZipCode, destNextFriday.Date);

            return timeInfo.Sunset;
        }

        private static int DaysUntilNextFriday(DateTime date)
        {
            int offset = DayOfWeek.Friday - date.DayOfWeek;

            if (offset <= 0) // next week
            {
                offset += 7;
            }

            return offset;
        }
    }
}
