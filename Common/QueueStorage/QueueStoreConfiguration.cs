namespace QueueStorage
{
    /// <summary>
    /// Configuration for a QueueStore
    /// </summary>
    public class QueueStoreConfiguration
    {
        /// <summary>
        /// Gets or sets the queue type
        /// </summary>
        public QueueStoreType Type { get; set; }

        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure queue name
        /// </summary>
        public string AzureQueueName { get; set; }
    }
}
