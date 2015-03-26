namespace KeyValueStorage.Tests
{
    using System;
    using KeyValueStorage.Tests.Fixtures;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Tests the Azure table key value store
    /// </summary>
    [TestClass]
    public class AzureTableKeyValueStoreTests : KeyValueStoreTests
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private const string ConnectionString = "UseDevelopmentStorage=true";
        
        /// <summary>
        /// The table name
        /// </summary>
        private string tableName = "test";

        /// <summary>
        /// Reset the Azure table store
        /// </summary>
        protected override void InitStore()
        {
            this.tableName = "test" + Guid.NewGuid().ToString("N").Substring(0, 8);
            
            AzureTableKeyValueStore<Dog> azureTableStore = new AzureTableKeyValueStore<Dog>();
            azureTableStore.InitAzureTable(ConnectionString, this.tableName);
            this.Store = azureTableStore;
        }

        /// <summary>
        /// Clean up the table after test run
        /// </summary>
        protected override void CleanUpStore()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
            CloudTableClient client = account.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(this.tableName);
            table.DeleteIfExists();
        }
    }
}
