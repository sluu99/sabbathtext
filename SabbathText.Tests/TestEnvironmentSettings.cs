namespace SabbathText.Tests
{
    using System;
    using System.Diagnostics;

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
    }
}
