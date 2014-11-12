using NodaTime;
using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class Sabbath
    {
        /// <summary>
        /// The system will still send out a Sabbath message if Sabbath has not lasted this long
        /// </summary>
        public static readonly TimeSpan SabbathMessageTimeSpan = TimeSpan.FromHours(20);

        /// <summary>
        /// Two Sabbath messages must be at least this far away
        /// </summary>
        public static readonly TimeSpan SabbathMessageGap = TimeSpan.FromDays(5);

        public Sabbath()
        {
            this.DataProvider = new AzureDataProvider();
        }

        public IDataProvider DataProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns>The upcoming Sabbath time for a specific location. The time is in UTC</returns>
        public async Task<DateTime> GetUpcomingSabbath(Location location)
        {
            DateTime now = Clock.UtcNow;
            DateTime destinationNow = location.GetLocalTime(now);
            
            LocationTimeInfo timeInfo = null;

            if (destinationNow.DayOfWeek == DayOfWeek.Friday)
            {
                timeInfo = await this.DataProvider.GetTimeInfoByZipCode(location.ZipCode, destinationNow.Date);

                if (timeInfo.Sunset > now) // the sun has not set yet
                {
                    return timeInfo.Sunset;
                }
            }
            
            DateTime destNextFriday = destinationNow.AddDays(DaysUntilNextFriday(destinationNow));
            timeInfo = await this.DataProvider.GetTimeInfoByZipCode(location.ZipCode, destNextFriday.Date);

            return timeInfo.Sunset;
        }

        
        public async Task<DateTime> GetLastSabbath(Location location)
        {
            DateTime now = Clock.UtcNow;
            DateTime destinationNow = location.GetLocalTime(now);

            LocationTimeInfo timeInfo = null;

            if (destinationNow.DayOfWeek == DayOfWeek.Friday)
            {
                // check if Sabath has already started
                timeInfo = await this.DataProvider.GetTimeInfoByZipCode(location.ZipCode, destinationNow.Date);

                if (timeInfo.Sunset < now) // sunset already
                {
                    return timeInfo.Sunset;
                }
            }

            DateTime destLastFriday = destinationNow.AddDays(-1 * DaysSinceLastFriday(destinationNow));
            timeInfo = await this.DataProvider.GetTimeInfoByZipCode(location.ZipCode, destLastFriday.Date);

            return timeInfo.Sunset;
        }

        private int DaysSinceLastFriday(DateTime date)
        {
            int offset = date.DayOfWeek - DayOfWeek.Friday;

            if (offset <= 0)
            {
                offset += 7;
            }

            return offset;
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
