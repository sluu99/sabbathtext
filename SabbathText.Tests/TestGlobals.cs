namespace SabbathText.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using KeyValueStorage;
    using QueueStorage;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;
using SabbathText.Tests.V1;

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
        /// The account store
        /// </summary>
        public static InMemoryKeyValueStore<AccountEntity> AccountStore = new InMemoryKeyValueStore<AccountEntity>();

        /// <summary>
        /// The message store
        /// </summary>
        public static InMemoryKeyValueStore<MessageEntity> MessageStore = new InMemoryKeyValueStore<MessageEntity>();

        /// <summary>
        /// The location store
        /// </summary>
        public static InMemoryKeyValueStore<LocationEntity> LocationStore = new InMemoryKeyValueStore<LocationEntity>();

        /// <summary>
        /// The ZIP code - account ID index store
        /// </summary>
        public static InMemoryKeyValueStore<ZipCodeAccountIdIndex> ZipCodeAccountIndices = new InMemoryKeyValueStore<ZipCodeAccountIdIndex>();

        /// <summary>
        /// The checkpoint queue
        /// </summary>
        public static InMemoryQueueStore CheckpointQueue = new InMemoryQueueStore();

        /// <summary>
        /// The message client
        /// </summary>
        public static InMemoryMessageClient MessageClient = new InMemoryMessageClient();

        /// <summary>
        /// The static constructor
        /// </summary>
        static TestGlobals()
        {
            AccountStore.InitMemory();
            MessageStore.InitMemory();
            LocationStore.InitMemory();
            ZipCodeAccountIndices.InitMemory();
            CheckpointStore.InitMemory();
            CheckpointQueue.InitMemory();
            MessageClient.InitMemory();
        }
    }
}
