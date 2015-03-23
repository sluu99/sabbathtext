namespace SabbathText.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using KeyValueStorage;
    using SabbathText.Compensation;
    using SabbathText.Entities;

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
        public static KeyValueStore<Checkpoint> CheckpointStore = new KeyValueStore<Checkpoint>();

        /// <summary>
        /// The identity store
        /// </summary>
        public static KeyValueStore<Identity> IdentityStore = new KeyValueStore<Identity>();

        /// <summary>
        /// The account store
        /// </summary>
        public static KeyValueStore<Account> AccountStore = new KeyValueStore<Account>();

        /// <summary>
        /// The static constructor
        /// </summary>
        static TestGlobals()
        {
            CheckpointStore.Init();
            IdentityStore.Init();
            AccountStore.Init();
        }
    }
}
