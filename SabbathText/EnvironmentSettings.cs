namespace SabbathText
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Environment settings
    /// </summary>
    public class EnvironmentSettings
    {
        /// <summary>
        /// Key for KeyValueStoreConnectionString
        /// </summary>
        protected const string KeyValueStoreConnectionStringKey = "KeyValueStoreConnectionString";

        /// <summary>
        /// Key for ServicePhoneNumber
        /// </summary>
        protected const string ServicePhoneNumberKey = "ServicePhoneNumber";

        private Dictionary<string, string> secrets;
        
        /// <summary>
        /// Make the constructor private
        /// </summary>
        /// <param name="secrets">The environment secrets.</param>
        protected EnvironmentSettings(Dictionary<string, string> secrets)
        {
            this.secrets = secrets;
        }

        /// <summary>
        /// Gets the key value store connection string
        /// </summary>
        public virtual string KeyValueStoreConnectionString
        {
            get
            {
                return this.secrets[KeyValueStoreConnectionStringKey];
            }
        }

        /// <summary>
        /// Gets the phone number used by this service
        /// </summary>
        public virtual string ServicePhoneNumber
        {
            get
            {
                return this.secrets[ServicePhoneNumberKey];
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

        /// <summary>
        /// Creates a new instance of <see cref="EnvironmentSettings"/> based on the environment name.
        /// </summary>
        /// <param name="environmentName">
        /// The environment name. If null is provided, the <c>SabbathTextEnvironment</c>
        /// environment variable will be used.
        /// </param>
        /// <returns>The environment settings.</returns>
        public static EnvironmentSettings Create(string environmentName = null)
        {
            if (environmentName == null)
            {
                environmentName = Environment.GetEnvironmentVariable("SabbathTextEnvironment");
            }

            switch (environmentName)
            {
                case "Production":
                    return new EnvironmentSettings(null);
                case "Staging":
                    return new StagingEnvironmentSettings();
            }

            return new DevEnvironmentSettings();
        }
    }
}
