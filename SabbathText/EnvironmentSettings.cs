namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using KeyValueStorage;
    using QueueStorage;
    using SabbathText.V1;

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

        /// <summary>
        /// Key for <see cref="TwilioAccount"/>
        /// </summary>
        protected const string TwilioAccountKey = "TwilioAccount";

        /// <summary>
        /// Key for <see cref="TwilioToken"/>
        /// </summary>
        protected const string TwilioTokenKey = "TwilioToken";

        /// <summary>
        /// Key for <see cref="IncomingMessagePrimaryToken"/>
        /// </summary>
        protected const string IncomingMessagePrimaryTokenKey = "IncomingMessagePrimaryToken";

        /// <summary>
        /// Key for <see cref="IncomingMessageSecondaryToken"/>
        /// </summary>
        protected const string IncomingMessageSecondaryTokenKey = "IncomingMessageSecondaryToken";

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
        /// Gets the <c>Twilio</c> account
        /// </summary>
        public virtual string TwilioAccount
        {
            get
            {
                return this.secrets[TwilioAccountKey];
            }
        }

        /// <summary>
        /// Gets the <c>Twilio</c> token
        /// </summary>
        public virtual string TwilioToken
        {
            get
            {
                return this.secrets[TwilioTokenKey];
            }
        }

        /// <summary>
        /// Gets the primary token for incoming message
        /// </summary>
        public virtual string IncomingMessagePrimaryToken
        {
            get
            {
                return this.secrets[IncomingMessagePrimaryTokenKey];
            }
        }

        /// <summary>
        /// Gets the secondary token for incoming message
        /// </summary>
        public virtual string IncomingMessageSecondaryToken
        {
            get
            {
                return this.secrets[IncomingMessageSecondaryTokenKey];
            }
        }

        /// <summary>
        /// Gets the message client type
        /// </summary>
        public virtual MessageClientType MessageClientType
        {
            get { return MessageClientType.Twilio; }
        }

        /// <summary>
        /// Gets the configuration for the account store.
        /// </summary>
        public virtual KeyValueStoreConfiguration AccountStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "accounts",
                };
            }
        }

        /// <summary>
        /// Gets the message store configuration.
        /// </summary>
        public virtual KeyValueStoreConfiguration MessageStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "messages",
                };
            }
        }

        /// <summary>
        /// Gets the location store configuration.
        /// </summary>
        public virtual KeyValueStoreConfiguration LocationStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "locations",
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the ZIP code - Account ID index store
        /// </summary>
        public virtual KeyValueStoreConfiguration ZipCodeAccountIdIndexStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "zipcodeaccountidindices",
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint store
        /// </summary>
        public virtual KeyValueStoreConfiguration CheckpointStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "checkpoints",
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint queue
        /// </summary>
        public virtual QueueStoreConfiguration CheckpointQueueConfiguration
        {
            get
            {
                return new QueueStoreConfiguration
                {
                    Type = QueueStoreType.AzureStorageQueue,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureQueueName = "checkpointqueue",
                };
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
        /// Gets the amount of time to way before trying again,
        /// if the checkpoint worker did not find any message.
        /// </summary>
        public TimeSpan CheckpointWorkerIdleDelay
        {
            get { return TimeSpan.FromSeconds(3); }
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
