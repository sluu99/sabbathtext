namespace SabbathText.Location.V1
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// This class contains information about a specific location.
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// Gets the ZIP code.
        /// </summary>
        public string ZipCode { get; internal set; }

        /// <summary>
        /// Gets the location type.
        /// </summary>
        public LocationType Type { get; internal set; }

        /// <summary>
        /// Gets the primary city of this location.
        /// </summary>
        public string PrimaryCity { get; internal set; }

        /// <summary>
        /// Gets the state abbreviation.
        /// </summary>
        public string State { get; internal set; }

        /// <summary>
        /// Gets the Alpha-2 country code.
        /// </summary>
        public string Country { get; internal set; }

        /// <summary>
        /// Gets the time zone name.
        /// </summary>
        public string TimeZoneName { get; internal set; }

        /// <summary>
        /// Gets the location latitude.
        /// </summary>
        public double Latitude { get; internal set; }

        /// <summary>
        /// Gets the location longitude.
        /// </summary>
        public double Longitude { get; internal set; }

        /// <summary>
        /// Gets the current time at the location
        /// </summary>
        public DateTime LocalTime
        {
            get
            {
                DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[this.TimeZoneName];
                return Instant.FromDateTimeUtc(Clock.UtcNow).InZone(timeZone).ToDateTimeUnspecified();
            }
        }

        /// <summary>
        /// Gets a <see cref="LocationInfo"/> instance from the ZIP Code.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns>The location information.</returns>
        public static LocationInfo FromZipCode(string zipCode)
        {
            Dictionary<string, string> rawData = RawData.GetZipCodeData(zipCode);

            if (rawData == null)
            {
                return null;
            }

            LocationInfo info = new LocationInfo
            {
                ZipCode = zipCode,
                PrimaryCity = rawData["primary_city"],
                State = rawData["state"],
                Country = rawData["country"],
                TimeZoneName = rawData["timezone"],
                Latitude = double.Parse(rawData["latitude"]),
                Longitude = double.Parse(rawData["longitude"]),
            };

            switch (rawData["type"].ToUpperInvariant())
            {
                case "STANDARD":
                    info.Type = LocationType.Standard;
                    break;
                case "MILITARY":
                    info.Type = LocationType.Military;
                    break;
                case "UNIQUE":
                    info.Type = LocationType.Unique;
                    break;
                case "PO BOX":
                    info.Type = LocationType.POBox;
                    break;
            }

            return info;
        }
    }
}
