namespace SabbathText.Tests.V1
{
    using System;
    using System.Linq;
    using System.Threading;
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
        /// Ensures that the account has the expected conversation context.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedContext">The expected conversation context.</param>
        protected static void AssertConversationContext(string accountId, ConversationContext expectedContext)
        {
            AccountEntity account = new AccountEntity
            {
                AccountId = accountId,
            };
            account = TestGlobals.AccountStore.Get(account.PartitionKey, account.RowKey).Result;
            Assert.AreEqual<ConversationContext>(expectedContext, account.ConversationContext);
        }

        /// <summary>
        /// Creates an operation context for a specific account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>An Operation context instance.</returns>
        protected static OperationContext CreateContext(AccountEntity account)
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
                Settings = TestGlobals.Settings,
            };
        }

        /// <summary>
        /// Creates a new OperationContext instance
        /// </summary>
        /// <returns>An OperationContext instance</returns>
        protected static OperationContext CreateContext()
        {
            return CreateContext(CreateAccount());
        }

        /// <summary>
        /// Creates a new test account.
        /// </summary>
        /// <returns>The test account.</returns>
        protected static AccountEntity CreateAccount()
        {
            string phoneNumber = TestHelper.GetUSPhoneNumber();
            string accountId = AccountEntity.GetAccountId(phoneNumber);

            return TestGlobals.AccountStore.InsertOrGet(new AccountEntity
            {
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
        protected static void AssertOperationFinishes(OperationContext context)
        {
            Checkpoint checkpoint = GetCheckpoint(context);
            Assert.IsNotNull(checkpoint, "Cannot find a checkpoint for this operation.");
            
            CompensationClient compensation = new CompensationClient(
                TestGlobals.CheckpointStore, TestGlobals.CheckpointQueue, TestGlobals.Settings.CheckpointVisibilityTimeout);
            OperationCheckpointHandler handler = new OperationCheckpointHandler(
                TestGlobals.AccountStore,
                TestGlobals.MessageStore,
                TestGlobals.MessageClient,
                compensation,
                context.Settings);
            CancellationTokenSource cts = new CancellationTokenSource(TestGlobals.Settings.OperationTimeout);
            handler.Finish(checkpoint, cts.Token).Wait();

            Assert.IsTrue(
                checkpoint.Status == CheckpointStatus.Completed || checkpoint.Status == CheckpointStatus.Cancelled,
                string.Format("Expected the checkpoint status to be Completed or Cancelled, but is actually {0}", checkpoint.Status));
        }

        /// <summary>
        /// Ensures that the account has a certain ZIP code.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedZipCode">The expected ZIP code.</param>
        protected static void AssertZipCode(string accountId, string expectedZipCode)
        {
            AccountEntity account = new AccountEntity
            {
                AccountId = accountId,
            };
            account = TestGlobals.AccountStore.Get(account.PartitionKey, account.RowKey).Result;
            Assert.AreEqual<string>(expectedZipCode, account.ZipCode);
        }

        /// <summary>
        /// Ensures that the last message sent to the user has a particular template.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="template">The message template.</param>
        /// <param name="mustContain">The message body must contain the provided string.</param>
        protected static void AssertLastSentMessage(string accountId, MessageTemplate template, string mustContain = null)
        {
            AccountEntity account = new AccountEntity
            {
                AccountId = accountId,
            };
            account = TestGlobals.AccountStore.Get(account.PartitionKey, account.RowKey).Result;
            Assert.IsTrue(
                account.RecentMessages.Count > 0,
                "Could not find any recent messages");

            MessageEntity lastMessage = account.RecentMessages[account.RecentMessages.Count - 1];
            Assert.AreEqual<MessageDirection>(MessageDirection.Outgoing, lastMessage.Direction);
            Assert.AreEqual<MessageStatus>(MessageStatus.Sent, lastMessage.Status);
            Assert.AreEqual<MessageTemplate>(template, lastMessage.Template);

            if (mustContain != null)
            {
                Assert.IsTrue(
                    lastMessage.Body.Contains(mustContain),
                    string.Format("The message body must contain '{0}'", mustContain));
            }
        }

        /// <summary>
        /// Asserts the number of messages with a certain templates went through the message client for a phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="template">The message template.</param>
        /// <param name="expectedCount">The expected count</param>
        protected static void AssertMessageCount(string phoneNumber, MessageTemplate template, int expectedCount)
        {
            int count = TestGlobals.MessageClient.Messages.Count(m =>
                m.Template == template &&
                (m.Sender == phoneNumber || m.Recipient == phoneNumber));

            Assert.AreEqual(
                expectedCount,
                count,
                string.Format("The phone number {0} is expected to have {1} {2} messages. Actual count: {3}", phoneNumber, expectedCount, template, count));
        }

        /// <summary>
        /// Gets a checkpoint from the operation context.
        /// </summary>
        /// <param name="context">The operation context</param>
        /// <returns>The checkpoint.</returns>
        private static Checkpoint GetCheckpoint(OperationContext context)
        {
            CheckpointReference checkpointRef = new CheckpointReference
            {
                PartitionKey = context.Account.AccountId,
                RowKey = context.TrackingId,
            };

            return context.Compensation.GetCheckpoint(checkpointRef).Result;
        }
    }
}
