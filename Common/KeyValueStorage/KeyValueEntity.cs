namespace KeyValueStorage
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents an entity from the key-value storage
    /// </summary>
    [Serializable]
    public abstract class KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the partition key
        /// </summary>
        [JsonProperty]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        [JsonProperty]
        public string RowKey { get; set; }
        
        /// <summary>
        /// Gets the e-tag
        /// </summary>
        [JsonProperty]
        public string ETag { get; internal set; }
        
        /// <summary>
        /// Gets the timestamp
        /// </summary>
        [JsonProperty]
        public DateTime Timestamp { get; internal set; }
    }
}
