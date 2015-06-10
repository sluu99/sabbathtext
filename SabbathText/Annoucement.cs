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
        public Annoucement()
        {
            this.IsEligible = (account) => true;
        }

        /// <summary>
        /// Gets or sets the  announcement ID
        /// </summary>
        public string AnnouncementId { get; set; }

        /// <summary>
        /// Gets or sets the announcement content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the function that check whether an account is eligible for the announcement
        /// </summary>
        public Func<AccountEntity, bool> IsEligible { get; set; }
    }
}
