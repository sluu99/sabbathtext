namespace SabbathText.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test assembly initialization and clean up
    /// </summary>
    [TestClass]
    public class TestAssembly
    {
        /// <summary>
        /// Initializes the test assembly
        /// </summary>
        /// <param name="context">The test context.</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            GoodieBag.Initialize(new TestEnvironmentSettings());
        }
    }
}
