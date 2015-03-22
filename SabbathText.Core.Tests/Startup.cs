using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Tests
{
    [TestClass]
    public static class Startup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Common.SetupStorage();
            // Common.Setup();
        }
    }
}
