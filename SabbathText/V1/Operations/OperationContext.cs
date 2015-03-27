namespace SabbathText.V1.Operations
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
        /// Gets or sets the entity store
        /// </summary>
        public InMemoryKeyValueStore<Identity> IdentityStore { get; set; }

        /// <summary>
        /// Gets or sets the account store
        /// </summary>
        public InMemoryKeyValueStore<Account> AccountStore { get; set; }

        /// <summary>
        /// Gets or sets the compensation client
        /// </summary>
        public CompensationClient Compensation { get; set; }
    }
}
