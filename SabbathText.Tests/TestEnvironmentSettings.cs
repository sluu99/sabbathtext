namespace SabbathText.Tests
{
    using System;
    using System.Diagnostics;
    using KeyValueStorage;
    using QueueStorage;

    /// <summary>
    /// Test environment settings
    /// </summary>
    public class TestEnvironmentSettings : EnvironmentSettings
    {
        /// <summary>
        /// Creates a new instance of test environment settings
        /// </summary>
        public TestEnvironmentSettings()
            : base(null)
        {
        }
        
        /// <summary>
        /// Gets the default operation timeout for the test environment
        /// </summary>
        public override TimeSpan OperationTimeout
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return TimeSpan.FromDays(1);
                }

                return base.OperationTimeout;
            }
        }

        /// <summary>
        /// Gets or sets the checkpoint invisibility timeout
        /// </summary>
        public override TimeSpan CheckpointVisibilityTimeout
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return TimeSpan.FromDays(1);
                }

                return base.CheckpointVisibilityTimeout;
            }
        }

        /// <summary>
        /// Gets the configuration for the account store.
        /// </summary>
        public override KeyValueStoreConfiguration AccountStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.InMemory
                };
            }
        }

        /// <summary>
        /// Gets the message store configuration.
        /// </summary>
        public override KeyValueStoreConfiguration MessageStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.InMemory
                };
            }
        }

        /// <summary>
        /// Gets the location store configuration.
        /// </summary>
        public override KeyValueStoreConfiguration LocationStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.InMemory
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the ZIP code - Account ID index store
        /// </summary>
        public override KeyValueStoreConfiguration ZipCodeAccountIdIndexStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.InMemory
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint store
        /// </summary>
        public override KeyValueStoreConfiguration CheckpointStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.InMemory
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint queue
        /// </summary>
        public override QueueStoreConfiguration CheckpointQueueConfiguration
        {
            get
            {
                return new QueueStoreConfiguration
                {
                    Type = QueueStoreType.InMemory
                };
            }
        }

        /// <summary>
        /// Gets the message client type
        /// </summary>
        public override SabbathText.V1.MessageClientType MessageClientType
        {
            get
            {
                return SabbathText.V1.MessageClientType.InMemory;
            }
        }
    }
}
