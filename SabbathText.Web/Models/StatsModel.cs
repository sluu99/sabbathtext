namespace SabbathText.Web.Models
{
    using System;

    /// <summary>
    /// Controller model for the stats page
    /// </summary>
    public class StatsModel
    {
        /// <summary>
        /// Gets or sets the number of account
        /// </summary>
        public long AccountCount { get; set; }

        /// <summary>
        /// Gets or sets the number of subscriber
        /// </summary>
        public long SubscriberCount { get; set; }

        /// <summary>
        /// Gets or sets the poison messages
        /// </summary>
        public int PoisonMessageCount { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the stats
        /// </summary>
        public DateTime StatsUpdatedOn { get; set; }
    }
}