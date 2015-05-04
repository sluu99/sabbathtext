namespace SabbathText.Compensation.V1
{
    using System.Globalization;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Checkpoint status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CheckpointStatus
    {
        /// <summary>
        /// The operation is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// The operation is completed
        /// </summary>
        Completed,

        /// <summary>
        /// The operation has been cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// The operation is in the process of rolling back
        /// </summary>
        Cancelling,

        /// <summary>
        /// The operation is marked to continue later
        /// </summary>
        DelayedProcessing,
    }

    /// <summary>
    /// Represents a checkpoint
    /// </summary>
    public class Checkpoint : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the account ID.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the tracking ID
        /// </summary>
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets the checkpoint status
        /// </summary>
        public CheckpointStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the operation type
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Gets or sets the checkpoint data
        /// </summary>
        public string CheckpointData { get; set; }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        public override string PartitionKey
        {
            get { return this.AccountId; }
        }

        /// <summary>
        /// Gets the row key.
        /// </summary>
        public override string RowKey
        {
            get { return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.OperationType, this.TrackingId); }
        }
    }
}
