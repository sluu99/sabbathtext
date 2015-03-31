namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.V1;

    /// <summary>
    /// The base class for tests
    /// </summary>
    [TestClass]
    public abstract class TestBase
    {
        /// <summary>
        /// Creates a new OperationContext instance
        /// </summary>
        /// <returns>An OperationContext instance</returns>
        protected OperationContext CreateContext()
        {
            return new OperationContext
            {
                TrackingId = Guid.NewGuid().ToString(),
                AccountStore = TestGlobals.AccountStore,
                CancellationToken = new CancellationTokenSource(TestGlobals.Settings.OperationTimeout).Token,
                Compensation = new Compensation.V1.CompensationClient(
                    TestGlobals.CheckpointStore, TestGlobals.CheckpointQueue, TestGlobals.Settings.CheckpointVisibilityTimeout),
                MessageClient = TestGlobals.MessageClient,
                MessageStore = TestGlobals.MessageStore,
            };
        }
    }
}
