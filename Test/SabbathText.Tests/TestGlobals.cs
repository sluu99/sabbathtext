namespace SabbathText.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using KeyValueStorage;
    using QueueStorage;
    using SabbathText.Compensation.V1;
    using SabbathText.V1.Entities;

    /// <summary>
    /// Test global variables
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed")]
    public static class TestGlobals
    {
        /// <summary>
        /// Test environment settings
        /// </summary>
        public static TestEnvironmentSettings Settings = new TestEnvironmentSettings();

        /// <summary>
        /// The checkpoint store
        /// </summary>
        public static InMemoryKeyValueStore<Checkpoint> CheckpointStore = new InMemoryKeyValueStore<Checkpoint>();

        /// <summary>
        /// The identity store
        /// </summary>
        public static InMemoryKeyValueStore<Identity> IdentityStore = new InMemoryKeyValueStore<Identity>();

        /// <summary>
        /// The account store
        /// </summary>
        public static InMemoryKeyValueStore<Account> AccountStore = new InMemoryKeyValueStore<Account>();

        /// <summary>
        /// The checkpoint queue
        /// </summary>
        public static InMemoryQueueStore CheckpointQueue = new InMemoryQueueStore();

        /// <summary>
        /// The static constructor
        /// </summary>
        static TestGlobals()
        {
            CheckpointStore.InitMemory();
            IdentityStore.InitMemory();
            AccountStore.InitMemory();
            CheckpointQueue.InitMemory();
        }
    }
}
