using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabbathText.Location.V1;

/// <summary>
/// Extension methods for <see cref="LocationInfo"/>
/// </summary>
public static class LocationInfoExtension
{
    /// <summary>
    /// Returns whether Sabbath has started at the location
    /// </summary>
    /// <param name="location">The location</param>
    /// <returns>Whether Sabbath has started at the location</returns>
    public static bool IsSabbath(this LocationInfo location)
    {
        if (location.LocalTime.DayOfWeek != DayOfWeek.Friday &&
            location.LocalTime.DayOfWeek != DayOfWeek.Saturday)
        {
            return false;
        }

        if (location.LocalTime.DayOfWeek == DayOfWeek.Friday)
        {
            TimeInfo fridayTimeInfo = TimeInfo.Create(location.ZipCode, location.LocalTime.Date);
            if (Clock.UtcNow < fridayTimeInfo.SunSetUtc)
            {
                // it's Friday, the sun has not set yet
                return false;
            }
        }
        else if (location.LocalTime.DayOfWeek == DayOfWeek.Saturday)
        {
            TimeInfo saturdayTimeInfo = TimeInfo.Create(location.ZipCode, location.LocalTime.Date);
            if (Clock.UtcNow > saturdayTimeInfo.SunSetUtc)
            {
                // it's Saturday, the sun has set, Sabbath ended
                return false;
            }
        }

        return true;
    }
}
