namespace SabbathText.Compensation.V1
{
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
    }

    /// <summary>
    /// Represents a checkpoint
    /// </summary>
    public class Checkpoint : KeyValueEntity
    {
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
    }
}
