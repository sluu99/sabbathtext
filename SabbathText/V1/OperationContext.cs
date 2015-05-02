﻿namespace SabbathText.V1
{
    using System.Threading;
    using KeyValueStorage;
    using QueueStorage;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;

    /// <summary>
    /// This class represents an operation context
    /// </summary>
    public class OperationContext
    {
        /// <summary>
        /// Gets or sets the tracking ID
        /// </summary>
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the operation
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the account of the operation
        /// </summary>
        public AccountEntity Account { get; set; }

        /// <summary>
        /// Gets or sets the account store
        /// </summary>
        public KeyValueStore<AccountEntity> AccountStore { get; set; }

        /// <summary>
        /// Gets or sets the message store
        /// </summary>
        public KeyValueStore<MessageEntity> MessageStore { get; set; }

        /// <summary>
        /// Gets or sets the location store
        /// </summary>
        public KeyValueStore<LocationEntity> LocationStore { get; set; }

        /// <summary>
        /// Gets or sets the index store for <see cref="ZipCodeAccountIdIndex"/>
        /// </summary>
        public KeyValueStore<ZipCodeAccountIdIndex> ZipCodeAccountIdIndices { get; set; }

        /// <summary>
        /// Gets or sets the compensation client
        /// </summary>
        public CompensationClient Compensation { get; set; }

        /// <summary>
        /// Gets or sets the message client
        /// </summary>
        public MessageClient MessageClient { get; set; }

        /// <summary>
        /// Gets or sets the settings
        /// </summary>
        public EnvironmentSettings Settings { get; set; }
    }
}
