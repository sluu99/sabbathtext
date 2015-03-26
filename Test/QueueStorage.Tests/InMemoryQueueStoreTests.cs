namespace QueueStorage.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Tests Azure implementation of the queue store
    /// </summary>
    [TestClass]
    public class InMemoryQueueStoreTests : QueueStoreTests
    {
        /// <summary>
        /// Initializes the queue store for testing
        /// </summary>
        protected override void InitStore()
        {
            InMemoryQueueStore store = new InMemoryQueueStore();
            store.InitMemory();
            this.Store = store;
        }

        /// <summary>
        /// Clean up the queue after test run
        /// </summary>
        protected override void CleanUpStore()
        {
        }
    }
}
