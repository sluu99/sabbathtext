﻿namespace SabbathText.Tests
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Test environment settings
    /// </summary>
    public class TestEnvironmentSettings : EnvironmentSettings
    {
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
    }
}