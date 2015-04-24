namespace SabbathText
{
    using System;

    /// <summary>
    /// Environment settings
    /// </summary>
    public class EnvironmentSettings
    {
        /// <summary>
        /// Gets the key value store connection string
        /// </summary>
        public virtual string KeyValueStoreConnectionString
        {
            get
            {
                return Environment.GetEnvironmentVariable("SabbathTextKeyValueStoreConnectionString");
            }
        }

        /// <summary>
        /// Gets the default operation timeout
        /// </summary>
        public virtual TimeSpan OperationTimeout
        {
            get { return TimeSpan.FromSeconds(30); }
        }

        /// <summary>
        /// Gets the timeout before the compensation agent picks up the checkpoint.
        /// This should be higher than the operation timeout
        /// </summary>
        public virtual TimeSpan CheckpointVisibilityTimeout
        {
            get { return TimeSpan.FromSeconds(60); }
        }

        /// <summary>
        /// Gets the amount of time gap between two Sabbath text messages.
        /// We cannot send more than one Sabbath message within this time span.
        /// </summary>
        public TimeSpan SabbathTextGap
        {
            get { return TimeSpan.FromDays(5); }
        }

        /// <summary>
        /// Gets the grace period for sending out Sabbath text.
        /// With in this time span after Sabbath started,
        /// we can still send out the Sabbath text.
        /// </summary>
        public TimeSpan SabbathTextGracePeriod
        {
            get { return TimeSpan.FromHours(22); }
        }

        /// <summary>
        /// Gets the number of messages to keep with the account entity before archiving.
        /// </summary>
        public int RecentMessageThreshold
        {
            get { return 200; }
        }
    }
}
