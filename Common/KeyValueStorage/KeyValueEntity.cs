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
        public abstract string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        [JsonProperty]
        public abstract string RowKey { get; set; }
        
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
