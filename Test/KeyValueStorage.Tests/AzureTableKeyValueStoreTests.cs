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
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=sluu99dev;AccountKey=Y2FgOw4QTv79BJ/gFNSTq7RekwOH4EwwmchrIq42uj4cFc3EqBCF/tOJikjQZYHXKq9Ziigbqf6yn0VjMARx7g==";
        
        /// <summary>
        /// The table name
        /// </summary>
        private string tableName = "test_";

        /// <summary>
        /// Reset the Azure table store
        /// </summary>
        protected override void InitStore()
        {
            this.tableName = "test" + Guid.NewGuid().ToString("N").Substring(8);
            this.Store = new AzureTableKeyValueStore<Dog>(ConnectionString, this.tableName);
            this.Store.Init();
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
