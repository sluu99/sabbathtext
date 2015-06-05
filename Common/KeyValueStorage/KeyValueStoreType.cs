namespace KeyValueStorage
{
    /// <summary>
    /// Different types of key value store
    /// </summary>
    public enum KeyValueStoreType
    {
        /// <summary>
        /// Uses in-memory storage
        /// </summary>
        InMemory,

        /// <summary>
        /// Uses Azure Table storage
        /// </summary>
        AzureTable,
    }
}
