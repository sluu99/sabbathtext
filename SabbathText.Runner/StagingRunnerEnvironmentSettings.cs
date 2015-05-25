namespace SabbathText.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using QueueStorage;

    /// <summary>
    /// Runner settings for the staging environment
    /// </summary>
    public class StagingRunnerEnvironmentSettings : StagingEnvironmentSettings
    {
        /// <summary>
        /// Gets the configuration for the checkpoint queue
        /// </summary>
        public override QueueStoreConfiguration CheckpointQueueConfiguration
        {
            get
            {
                QueueStoreConfiguration baseConfig = base.CheckpointQueueConfiguration;
                baseConfig.AzureQueueName = "runnercheckpointqueue";

                return baseConfig;
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint store
        /// </summary>
        public override KeyValueStoreConfiguration CheckpointStoreConfiguration
        {
            get
            {
                KeyValueStoreConfiguration baseConfig = base.CheckpointStoreConfiguration;
                baseConfig.AzureTableName = "runnercheckpoints";

                return baseConfig;
            }
        }
    }
}
