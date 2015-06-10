namespace SabbathText
{
    using System;
    using SabbathText.Entities;

    /// <summary>
    /// Represents an announcement
    /// </summary>
    public class Annoucement
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="announcementId">The announcement ID</param>
        /// <param name="content">The announcement content</param>
        /// <param name="isEligibleFunc">The function to determine whether an account is eligible for the announcement</param>
        public Annoucement(string announcementId, string content, Func<AccountEntity, bool> isEligibleFunc)
        {
            this.AnnouncementId = announcementId;
            this.Content = content;
            this.IsEligible = isEligibleFunc;
        }

        /// <summary>
        /// Gets the  announcement ID
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
