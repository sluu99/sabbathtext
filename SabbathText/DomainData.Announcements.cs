namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SabbathText.Entities;

    /// <summary>
    /// The announcements portion of the domain data
    /// </summary>
    public static partial class DomainData
    {
        /// <summary>
        /// The domain data for announcements
        /// </summary>
        public static readonly IEnumerable<Annoucement> Announcements = new Annoucement[]
        {
            new Annoucement
            {
                AnnouncementId = "BibleVerseAnnouncement",
                Content = "Did you know, you can get a Bible verse at any time by texting \"Bible verse\". Give it a try!",
                IsEligible = (account) =>
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

                    return true;
                },
            },
        };
    }
}
