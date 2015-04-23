namespace SabbathText.Tests.V1
{
    using System;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for ProcessMessageOperation
    /// </summary>
    [TestClass]
    public class ProcessMessageOperationTests : TestBase
    {
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
        /// Sends a greeting message to a specific account.
        /// </summary>
        /// <param name="account">The account.</param>
        protected static void GreetUser(AccountEntity account)
        {
            OperationContext context = CreateContext(account);
            GreetUserOperation operation = new GreetUserOperation(context);
            OperationResponse<bool> response = operation.Run().Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.Accepted, response.StatusCode);
            AssertOperationFinishes(context);
        }

        /// <summary>
        /// Processes an incoming message.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="incomingMessage">The incoming message.</param>
        protected static void ProcessMessage(string accountId, Message incomingMessage)
        {
            AccountEntity account = new AccountEntity
            {
                AccountId = accountId,
            };
            account = TestGlobals.AccountStore.Get(account.PartitionKey, account.RowKey).Result;

            OperationContext context = CreateContext(account);
            MessageProcessor processor = new MessageProcessor();
            processor.Process(context, incomingMessage).Wait();
            AssertOperationFinishes(context);
        }
    }
}
