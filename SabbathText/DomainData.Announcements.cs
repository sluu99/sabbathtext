namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SabbathText.Entities;
    using SabbathText.Location.V1;

    /// <summary>
    /// The announcements portion of the domain data
    /// </summary>
    public static partial class DomainData
    {
        /// <summary>
        /// The domain data for announcements
        /// </summary>
        public static readonly IEnumerable<Announcement> Announcements = new Announcement[]
        {
            CreateBibleVerseAnnouncement(),
            CreateDoubleTextBug20150627Announcement(),
        };

        private static Announcement CreateDoubleTextBug20150627Announcement()
        {
            return new Announcement(
                "DoubleTextBug20150627",
                "Hope you enjoyed the double doze of Sabbath texts! There was a glitch in our system, and it has been fixed. Sorry for the spam.",
                true,
                (account) =>
                {
                    if (account.Status != AccountStatus.Subscribed || string.IsNullOrWhiteSpace(account.ZipCode))
                    {
                        return false;
                    }

                    if (account.CreationTime > new DateTime(2015, 6, 26, 0, 0, 0, DateTimeKind.Utc))
                    {
                        return false;
                    }

                    return true;
                });
        }

        private static Announcement CreateBibleVerseAnnouncement()
        {
            return new Announcement(
                "BibleVerseAnnouncement",
                "Did you know, you can get a Bible verse at any time by texting \"Bible verse\". Give it a try!",
                (account) =>
                {
                    if (account.Status != AccountStatus.Subscribed || string.IsNullOrWhiteSpace(account.ZipCode))
                    {
                        return false;
                    }

                    if (Clock.UtcNow - account.CreationTime < TimeSpan.FromDays(7))
                    {
                        // the account is not 7 days old yet
                        return false;
                    }

                    LocationInfo locationInfo = LocationInfo.FromZipCode(account.ZipCode);
                    TimeInfo timeInfo = TimeInfo.Create(account.ZipCode, locationInfo.LocalTime.Date);

                    if (locationInfo.LocalTime.DayOfWeek != DayOfWeek.Tuesday &&
                        locationInfo.LocalTime.DayOfWeek != DayOfWeek.Wednesday)
                    {
                        // only send out this announcement on Tuesday & Wednesday
                        return false;
                    }

                    if (timeInfo.SunSetUtc < Clock.UtcNow)
                    {
                        // sunset already
                        return false;
                    }

                    if (timeInfo.SunSetUtc - Clock.UtcNow > TimeSpan.FromHours(5))
                    {
                        // sunset is more than 5 hours away
                        return false;
                    }

                    if (timeInfo.SunSetUtc - Clock.UtcNow < TimeSpan.FromHours(2.5))
                    {
                        // sunset is less than 4 hours away
                        return false;
                    }

                    return true;
                });
        }
    }
}
