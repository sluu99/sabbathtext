namespace SabbathText.Tests.V1
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using KeyValueStorage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using QueueStorage;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;
    using SabbathText.Runner;
    using SabbathText.V1;

    /// <summary>
    /// The base class for tests
    /// </summary>
    [TestClass]
    public abstract class TestBase
    {
        private FakeClockScope fakeClock;

        /// <summary>
        /// Runs before each test case begins
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            GoodieBag.Initialize(new TestEnvironmentSettings());
            GoodieBag.CreateFunc = null;
            this.fakeClock = new FakeClockScope();
        }

        /// <summary>
        /// Clean up after test cases
        /// </summary>
        [TestCleanup]
        public void TestCleanUp()
        {
            this.fakeClock.Dispose();
        }

        /// <summary>
        /// Ensures that the account has a certain status
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedStatus">The expected account status.</param>
        protected static void AssertAccountStatus(string accountId, AccountStatus expectedStatus)
        {
            GoodieBag bag = GoodieBag.Create();
            AccountEntity account = bag.AccountStore.Get(AccountEntity.GetReferenceById(accountId), CancellationToken.None).Result;
            Assert.AreEqual<AccountStatus>(expectedStatus, account.Status);
        }

        /// <summary>
        /// Ensures that the account has the expected conversation context.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedContext">The expected conversation context.</param>
        protected static void AssertConversationContext(string accountId, ConversationContext expectedContext)
        {
            GoodieBag bag = GoodieBag.Create();
            AccountEntity account = bag.AccountStore.Get(AccountEntity.GetReferenceById(accountId), CancellationToken.None).Result;
            Assert.AreEqual<ConversationContext>(expectedContext, account.ConversationContext);
        }

        /// <summary>
        /// Creates an operation context for a specific account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>An Operation context instance.</returns>
        protected static OperationContext CreateContext(AccountEntity account)
        {
            GoodieBag goodieBag = GoodieBag.Create();

            return new OperationContext
            {
                TrackingId = Guid.NewGuid().ToString(),
                Account = account,
                CancellationToken = new CancellationTokenSource(goodieBag.Settings.OperationTimeout).Token,
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
            string accountId = AccountEntity.GetAccountIdByPhoneNumber(phoneNumber);

            return GoodieBag.Create().AccountStore.InsertOrGet(
                new AccountEntity
                {
                    AccountId = accountId,
                    CreationTime = Clock.UtcNow,
                    PhoneNumber = phoneNumber,
                    Status = AccountStatus.BrandNew,
                    ConversationContext = ConversationContext.Unknown,
                },
                CancellationToken.None).Result;
        }

        /// <summary>
        /// Gets an account
        /// </summary>
        /// <param name="accountId">The account Id.</param>
        /// <returns>The account.</returns>
        protected static AccountEntity GetAccount(string accountId)
        {
            return GoodieBag.Create().AccountStore.Get(
                AccountEntity.GetReferenceById(accountId),
                CancellationToken.None)
                .Result;
        }
        
        /// <summary>
        /// Ensures an operation is finished base on the checkpoint.
        /// </summary>
        protected static void RunCheckpointWorker()
        {
            // wait 1 second to make sure hand off items can be picked up
            Clock.Delay(TimeSpan.FromSeconds(1)).Wait();

            GoodieBag bag = GoodieBag.Create();

            CheckpointWorker worker = new CheckpointWorker(
                bag.CompensationClient,
                bag.Settings.CheckpointWorkerIdleDelay,
                new OperationCheckpointHandler());

            // process all the checkpoints avail
            while (worker.RunIteration(CancellationToken.None).Result == TimeSpan.Zero)
            {
            }
        }

        /// <summary>
        /// Runs the checkpoint worker after the checkpoint lock expires.
        /// </summary>
        protected static void RunCheckpointWorkerAfterCheckpointLock()
        {
            GoodieBag bag = GoodieBag.Create();
            Clock.Delay(bag.Settings.CheckpointVisibilityTimeout);
            RunCheckpointWorker();
        }

        /// <summary>
        /// Ensures that the last message sent to the user has a particular template.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="template">The message template.</param>
        /// <param name="mustContain">The message body must contain the provided string.</param>
        protected static void AssertLastSentMessage(string accountId, MessageTemplate template, string mustContain = null)
        {
            AccountEntity account =
                GoodieBag.Create().AccountStore.Get(AccountEntity.GetReferenceById(accountId), CancellationToken.None).Result;
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
            int count = ((InMemoryMessageClient)GoodieBag.Create().MessageClient).Messages.Count(m =>
                m.Template == template &&
                (m.Sender == phoneNumber || m.Recipient == phoneNumber));

            Assert.AreEqual(
                expectedCount,
                count,
                string.Format("The phone number {0} is expected to have {1} {2} messages. Actual count: {3}", phoneNumber, expectedCount, template, count));
        }

        /// <summary>
        /// Creates an incoming message.
        /// </summary>
        /// <param name="sender">The sender phone number.</param>
        /// <param name="body">The message body.</param>
        /// <returns>The message.</returns>
        protected static Message CreateIncomingMessage(string sender, string body)
        {
            return new Message
            {
                Body = body,
                ExternalId = Guid.NewGuid().ToString(),
                Recipient = null,
                Sender = sender,
                Template = MessageTemplate.FreeForm,
                Timestamp = Clock.UtcNow,
            };
        }
        
        /// <summary>
        /// Inspect an <see cref="InspectAccountOperation"/> and pushes it to finish by invoking the compensation agent.
        /// </summary>
        /// <param name="accountId">The account ID</param>
        protected static void InspectAccount(string accountId)
        {
            OperationContext context = CreateContext(GetAccount(accountId));
            InspectAccountOperation operation = new InspectAccountOperation(context);
            OperationResponse<bool> response = operation.Run().Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.Accepted, response.StatusCode);
            RunCheckpointWorker();
        }

        /// <summary>
        /// Processes an incoming message.
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <returns>The operation response from processing the message.</returns>
        protected static OperationResponse<bool> ProcessMessage(Message incomingMessage)
        {
            string accountId = AccountEntity.GetAccountIdByPhoneNumber(incomingMessage.Sender);
            AccountEntity account =
                GoodieBag.Create().AccountStore.Get(AccountEntity.GetReferenceById(accountId), CancellationToken.None).Result;

            OperationContext context = CreateContext(account);
            context.TrackingId = incomingMessage.ExternalId;

            MessageProcessor processor = new MessageProcessor();
            return processor.Process(context, incomingMessage).Result;
        }
    }
}
