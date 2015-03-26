namespace QueueStorage.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Tests Azure implementation of the queue store
    /// </summary>
    [TestClass]
    public class AzureQueueStoreTests : QueueStoreTests
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private const string ConnectionString = "UseDevelopmentStorage=true";

        /// <summary>
        /// The queue name
        /// </summary>
        private string queueName = "test";

        /// <summary>
        /// Initializes the queue store for testing
        /// </summary>
        protected override void InitStore()
        {
            this.queueName = "test" + Guid.NewGuid().ToString("N").Substring(0, 8);
            AzureQueueStore store = new AzureQueueStore();
            store.InitAzureQueue(ConnectionString, this.queueName);
            this.Store = store;
        }

        /// <summary>
        /// Clean up the queue after test run
        /// </summary>
        protected override void CleanUpStore()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
            account.CreateCloudQueueClient().GetQueueReference(this.queueName).DeleteIfExists();
        }
    }
}
