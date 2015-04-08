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
        /// Creates an operation context for a specific account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>An Operation context instance.</returns>
        protected OperationContext CreateContext(AccountEntity account)
        {
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
        /// Creates a new OperationContext instance
        /// </summary>
        /// <returns>An OperationContext instance</returns>
        protected OperationContext CreateContext()
        {
            return this.CreateContext(this.CreateAccount());
        }

        /// <summary>
        /// Creates a new test account.
        /// </summary>
        /// <returns>The test account.</returns>
        protected AccountEntity CreateAccount()
        {
            string phoneNumber = TestHelper.GetUSPhoneNumber();
            string accountId = AccountEntity.GetAccountId(phoneNumber);

            return TestGlobals.AccountStore.InsertOrGet(new AccountEntity
            {
                PartitionKey = accountId,
                RowKey = accountId,
                AccountId = accountId,
                CreationTime = Clock.UtcNow,
                PhoneNumber = phoneNumber,
                Status = AccountStatus.BrandNew,
                ConversationContext = ConversationContext.Unknown,
            }).Result;
        }

        /// <summary>
        /// Ensures an operation is finished base on the checkpoint.
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
        /// Ensures that the account has the expected conversation context.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedContext">The expected conversation context.</param>
        protected void AssertConversationContext(string accountId, ConversationContext expectedContext)
        {
            AccountEntity account = TestGlobals.AccountStore.Get(accountId, accountId).Result;
            Assert.AreEqual<ConversationContext>(expectedContext, account.ConversationContext);
        }

        /// <summary>
        /// Ensures that the last message sent to the user has a particular template.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="template">The message template.</param>
        protected void AssertLastSentMessage(string accountId, MessageTemplate template)
        {
            AccountEntity account = TestGlobals.AccountStore.Get(accountId, accountId).Result;
            Assert.IsTrue(
                account.RecentMessages.Count > 0,
                "Could not find any recent messages");

            MessageEntity lastMessage = account.RecentMessages[account.RecentMessages.Count - 1];
            Assert.AreEqual<MessageDirection>(MessageDirection.Outgoing, lastMessage.Direction);
            Assert.AreEqual<MessageStatus>(MessageStatus.Sent, lastMessage.Status);
            Assert.AreEqual<MessageTemplate>(template, lastMessage.Template);
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
