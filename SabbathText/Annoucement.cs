namespace SabbathText
{
    using System;
    using SabbathText.Entities;

    /// <summary>
    /// Represents an announcement
    /// </summary>
    public class Announcement
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="announcementId">The announcement ID</param>
        /// <param name="content">The announcement content</param>
        /// <param name="isEligibleFunc">The function to determine whether an account is eligible for the announcement</param>
        public Announcement(string announcementId, string content, Func<AccountEntity, bool> isEligibleFunc)
            : this(announcementId, content, false, isEligibleFunc)
        {
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="announcementId">The announcement ID</param>
        /// <param name="content">The announcement content</param>
        /// <param name="overrideAnnouncementGap">Can this announcement override the announcement gap.</param>
        /// <param name="isEligibleFunc">The function to determine whether an account is eligible for the announcement</param>
        public Announcement(string announcementId, string content, bool overrideAnnouncementGap, Func<AccountEntity, bool> isEligibleFunc)
        {
            this.AnnouncementId = announcementId;
            this.Content = content;
            this.IsEligible = isEligibleFunc;
            this.OverrideAnnouncementGap = overrideAnnouncementGap;
        }

        /// <summary>
        /// Gets a value indicating whether the announcement can override the announcement time gap.
        /// </summary>
        public bool OverrideAnnouncementGap { get; private set; }

        /// <summary>
        /// Gets the announcement ID
        /// </summary>
        public string AnnouncementId { get; private set; }

        /// <summary>
        /// Gets the announcement content
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the function that check whether an account is eligible for the announcement
        /// </summary>
        public Func<AccountEntity, bool> IsEligible { get; private set; }
    }
}
