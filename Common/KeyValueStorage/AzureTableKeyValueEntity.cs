namespace KeyValueStorage
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// The internal azure table entity used to store KeyValueEntity
    /// </summary>
    internal class AzureTableKeyValueEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the serialized entity data
        /// </summary>
        public string EntityData { get; set; }
    }
}
