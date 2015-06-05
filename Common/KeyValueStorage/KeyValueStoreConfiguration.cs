namespace KeyValueStorage
{
    /// <summary>
    /// Contains the configuration for a key value store
    /// </summary>
    public class KeyValueStoreConfiguration
    {
        /// <summary>
        /// Gets or sets the key value store type
        /// </summary>
        public KeyValueStoreType Type { get; set; }

        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure table name
        /// </summary>
        public string AzureTableName { get; set; }
    }
}
