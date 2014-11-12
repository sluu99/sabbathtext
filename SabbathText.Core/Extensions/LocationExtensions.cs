using NodaTime;
using SabbathText.Core.Entities;
using System;

public static class LocationExtensions
{
    public static DateTime GetLocalTime(this Location location, DateTime utc)
    {
        DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[location.TimeZoneName];
        return Instant.FromDateTimeUtc(utc).InZone(timeZone).ToDateTimeUnspecified();
    }
}
