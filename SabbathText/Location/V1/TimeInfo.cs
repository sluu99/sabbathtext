namespace SabbathText.Location.V1
{
    using System;
    using NodaTime;

    /// <summary>
    /// Contains time information for a location
    /// </summary>
    public class TimeInfo
    {
        /// <summary>
        /// Gets the ZIP code.
        /// </summary>
        public string ZipCode { get; internal set; }

        /// <summary>
        /// Gets the date of the time information.
        /// </summary>
        public DateTime TimeInfoDate { get; internal set; }

        /// <summary>
        /// Gets the Sun set time in UTC.
        /// </summary>
        public DateTime SunSetUtc { get; internal set; }

        /// <summary>
        /// Gets the local Sun set time of the location.
        /// </summary>
        public DateTime SunSetDestination { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="TimeInfo"/> based on the ZIP code and the provided date.
        /// </summary>
        /// <param name="zipCode">The ZIP code.</param>
        /// <param name="date">The date of the time information.</param>
        /// <returns>The time information, or null if the ZIP code does not exist.</returns>
        public static TimeInfo Create(string zipCode, DateTime date)
        {
            LocationInfo locationInfo = LocationInfo.FromZipCode(zipCode);
            
            if (locationInfo == null)
            {
                return null;
            }

            TimeInfo timeInfo = new TimeInfo
            {
                ZipCode = zipCode,
                TimeInfoDate = date.Date,
            };

            timeInfo.SunSetUtc = SunCalculator.CalculateSunSetUtc(
                date.Year,
                date.Month,
                date.Day,
                locationInfo.Latitude,
                locationInfo.Longitude);

            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[locationInfo.TimeZoneName];
            timeInfo.SunSetDestination = Instant.FromDateTimeUtc(timeInfo.SunSetUtc).InZone(timeZone).ToDateTimeUnspecified();

            return timeInfo;
        }
    }
}
