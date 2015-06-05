namespace QueueStorage
{
    /// <summary>
    /// Different types of queue store
    /// </summary>
    public enum QueueStoreType
    {
        /// <summary>
        /// Uses the in-memory queue
        /// </summary>
        InMemory,

        /// <summary>
        /// Uses the Azure storage queue
        /// </summary>
        AzureStorageQueue,
    }
}
