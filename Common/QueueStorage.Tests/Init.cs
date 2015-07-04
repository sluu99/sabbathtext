namespace QueueStorage.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Initialization class for tests
    /// </summary>
    [TestClass]
    public static class Init
    {
        private static readonly string[] StorageEmulatorPaths = new string[]
        {
            Path.Combine(
                Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                @"Microsoft SDKs\Azure\Storage Emulator\WAStorageEmulator.exe"),
            Path.Combine(
                Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                @"Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe"),
        };        

        private static readonly string[] StorageEmulatorProcessNames = { "WAStorageEmulator", "WASTOR~1", "AzureStorageEmulator", "AZURES~1" };

        /// <summary>
        /// Initialization for all the tests in this assembly
        /// </summary>
        /// <param name="context">The test context</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            if (!IsStorageEmulatorStarted())
            {
                foreach (string emulatorPath in StorageEmulatorPaths)
                {
                    if (File.Exists(emulatorPath) == false)
                    {
                        continue;
                    }
                    
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = emulatorPath,
                        Arguments = "start",
                        CreateNoWindow = true,
                    };

                    using (Process proc = Process.Start(startInfo))
                    {
                        proc.WaitForExit();
                    }
                    
                    break;
                }
            }
        }

        private static bool IsStorageEmulatorStarted()
        {
            return StorageEmulatorProcessNames.Any(name => (Process.GetProcessesByName(name).FirstOrDefault() != null));
        }
    }
}
