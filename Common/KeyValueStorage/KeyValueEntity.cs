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
        /// Gets the partition key
        /// </summary>
        [JsonIgnore]
        public abstract string PartitionKey { get; }

        /// <summary>
        /// Gets the row key
        /// </summary>
        [JsonIgnore]
        public abstract string RowKey { get; }
        
        /// <summary>
        /// Gets or sets the e-tag
        /// </summary>
        [JsonProperty]
        public string ETag { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        [JsonProperty]
        public DateTime Timestamp { get; set; }
    }
}
