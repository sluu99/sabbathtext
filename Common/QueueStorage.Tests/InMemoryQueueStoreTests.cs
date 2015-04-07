namespace QueueStorage.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests Azure implementation of the queue store
    /// </summary>
    [TestClass]
    public class InMemoryQueueStoreTests : QueueStoreTests
    {
        /// <summary>
        /// This method will be called before every test run
        /// </summary>
        [TestInitialize]
        public override void Init()
        {
            base.Init();
            Clock.ResetClock();
            Clock.UseFakeClock();
        }

        /// <summary>
        /// this method will be called after test runs
        /// </summary>
        [TestCleanup]
        public override void CleanUp()
        {
            base.CleanUp();
            Clock.ResetClock();
            Clock.UseSystemClock();
        }

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
