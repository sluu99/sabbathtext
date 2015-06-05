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
        private static readonly string WAStorageEmulatorPath = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
            @"Microsoft SDKs\Azure\Storage Emulator\WAStorageEmulator.exe");

        private static readonly string[] WAStorageEmulatorProcessNames = { "WAStorageEmulator", "WASTOR~1" };

        /// <summary>
        /// Initialization for all the tests in this assembly
        /// </summary>
        /// <param name="context">The test context</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            if (!IsWAStorageEmulatorStarted())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = WAStorageEmulatorPath,
                    Arguments = "start",
                    CreateNoWindow = true,
                };

                using (Process proc = Process.Start(startInfo))
                {
                    proc.WaitForExit();
                }
            }
        }

        private static bool IsWAStorageEmulatorStarted()
        {
            return WAStorageEmulatorProcessNames.Any(name => (Process.GetProcessesByName(name).FirstOrDefault() != null));
        }
    }
}
