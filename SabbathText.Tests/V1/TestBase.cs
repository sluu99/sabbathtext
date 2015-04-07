namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;
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
            string phoneNumber = TestHelper.GetUSPhoneNumber();
            string accountId = AccountEntity.GetAccountId(phoneNumber);

            AccountEntity account = TestGlobals.AccountStore.InsertOrGet(new AccountEntity
            {
                PartitionKey = accountId,
                RowKey = accountId,
                AccountId = accountId,
                CreationTime = Clock.UtcNow,
                PhoneNumber = phoneNumber,
                Status = AccountStatus.BrandNew,
            }).Result;

            return new OperationContext
            {
                TrackingId = Guid.NewGuid().ToString(),
                Account = account,
                CancellationToken = new CancellationTokenSource(TestGlobals.Settings.OperationTimeout).Token,
                Compensation = new CompensationClient(
                    TestGlobals.CheckpointStore, TestGlobals.CheckpointQueue, TestGlobals.Settings.CheckpointVisibilityTimeout),
                MessageClient = TestGlobals.MessageClient,
                MessageStore = TestGlobals.MessageStore,
                AccountStore = TestGlobals.AccountStore,
            };
        }

        /// <summary>
        /// Ensure an operation is finished base on the checkpoint.
        /// </summary>
        /// <param name="context">The operation context.</param>
        protected void AssertOperationFinishes(OperationContext context)
        {
            Checkpoint checkpoint = this.GetCheckpoint(context);
            Assert.IsNotNull(checkpoint, "Cannot find a checkpoint for this operation.");
            
            CompensationClient compensation = new CompensationClient(
                TestGlobals.CheckpointStore, TestGlobals.CheckpointQueue, TestGlobals.Settings.CheckpointVisibilityTimeout);
            OperationCheckpointHandler handler = new OperationCheckpointHandler(
                TestGlobals.AccountStore,
                TestGlobals.MessageStore,
                TestGlobals.MessageClient,
                compensation);
            CancellationTokenSource cts = new CancellationTokenSource(TestGlobals.Settings.OperationTimeout);
            handler.Finish(checkpoint, cts.Token).Wait();

            Assert.IsTrue(
                checkpoint.Status == CheckpointStatus.Completed || checkpoint.Status == CheckpointStatus.Cancelled,
                string.Format("Expected the checkpoint status to be Completed or Cancelled, but is actually {0}", checkpoint.Status));
        }

        /// <summary>
        /// Gets a checkpoint from the operation context.
        /// </summary>
        /// <param name="context">The operation context</param>
        /// <returns>The checkpoint.</returns>
        private Checkpoint GetCheckpoint(OperationContext context)
        {
            CheckpointReference checkpointRef = new CheckpointReference
            {
                PartitionKey = context.Account.AccountId,
                RowKey = context.TrackingId.Sha256(),
            };

            return context.Compensation.GetCheckpoint(checkpointRef).Result;
        }
    }
}
