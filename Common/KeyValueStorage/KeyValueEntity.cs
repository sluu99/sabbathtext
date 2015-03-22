namespace KeyValueStorage
{
    using System;

    /// <summary>
    /// This class represents an entity from the key-value storage
    /// </summary>
    public abstract class KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the partition key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        public string RowKey { get; set; }
        
        /// <summary>
        /// Gets the e-tag
        /// </summary>
        public string ETag { get; internal set; }
        
        /// <summary>
        /// Gets the timestamp
        /// </summary>
        public DateTime Timestamp { get; internal set; }
    }
}
