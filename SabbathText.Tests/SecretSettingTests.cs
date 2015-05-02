﻿namespace SabbathText.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The main purpose of this class is to make sure secret settings work as expected (with encryption/decryption).
    /// </summary>
    [TestClass]
    public class SecretSettingTests
    {
        /// <summary>
        /// Tests connection string
        /// </summary>
        [TestMethod]
        public void Settings_ConnectionString()
        {
            EnvironmentSettings settings = EnvironmentSettings.Create();
            Assert.AreEqual("UseDevelopmentStorage=true", settings.KeyValueStoreConnectionString);
        }
    }
}
