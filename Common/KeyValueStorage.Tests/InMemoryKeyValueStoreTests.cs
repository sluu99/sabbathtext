namespace KeyValueStorage.Tests
{
    using KeyValueStorage.Tests.Fixtures;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the Azure table key value store
    /// </summary>
    [TestClass]
    public class InMemoryKeyValueStoreTests : KeyValueStoreTests
    {
        /// <summary>
        /// Reset the Azure table store
        /// </summary>
        protected override void InitStore()
        {
            InMemoryKeyValueStore<Dog> store = new InMemoryKeyValueStore<Dog>();
            store.InitMemory();
            this.Store = store;
        }

        /// <summary>
        /// Clean up the table after test run
        /// </summary>
        protected override void CleanUpStore()
        {
        }
    }
}
