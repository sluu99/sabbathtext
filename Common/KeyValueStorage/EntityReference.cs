namespace KeyValueStorage
{
    /// <summary>
    /// A class referencing an entity
    /// </summary>
    public class EntityReference
    {
        /// <summary>
        /// Gets or sets the partition key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        public string RowKey { get; set; }
    }
}
