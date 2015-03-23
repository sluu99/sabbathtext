﻿namespace SabbathText.Operations
{
    using System.Threading;
    using KeyValueStorage;
    using SabbathText.Compensation;
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
        /// Gets or sets the entity store
        /// </summary>
        public KeyValueStore<Identity> IdentityStore { get; set; }

        /// <summary>
        /// Gets or sets the account store
        /// </summary>
        public KeyValueStore<Account> AccountStore { get; set; }

        /// <summary>
        /// Gets or sets the checkpoint store
        /// </summary>
        public KeyValueStore<Checkpoint> CheckpointStore { get; set; }
    }
}
