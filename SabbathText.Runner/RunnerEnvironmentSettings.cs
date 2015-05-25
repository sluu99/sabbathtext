namespace SabbathText.Runner
{
    using System;
    using KeyValueStorage;
    using QueueStorage;

    /// <summary>
    /// Settings for runners
    /// </summary>
    public class RunnerEnvironmentSettings : EnvironmentSettings
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        protected RunnerEnvironmentSettings()
            : base(null)
        {
        }

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

        /// <summary>
        /// Creates a new instance of <see cref="EnvironmentSettings"/> for the runner.
        /// </summary>
        /// <param name="environmentName">
        /// The environment name. If null is provided, the <c>SabbathTextEnvironment</c>
        /// environment variable will be used.
        /// </param>
        /// <returns>The environment settings.</returns>
        public static EnvironmentSettings Create(string environmentName = null)
        {
            if (environmentName == null)
            {
                environmentName = Environment.GetEnvironmentVariable("SabbathTextEnvironment");
            }

            switch (environmentName)
            {
                case "Production":
                    return new RunnerEnvironmentSettings();
                case "Staging":
                    return new StagingRunnerEnvironmentSettings();
            }

            return new DevRunnerEnvironmentSettings();
        }
    }
}
