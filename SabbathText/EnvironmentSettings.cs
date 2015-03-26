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
        public virtual TimeSpan CheckpointInvisibilityTimeout
        {
            get { return TimeSpan.FromSeconds(60); }
        }
    }
}
