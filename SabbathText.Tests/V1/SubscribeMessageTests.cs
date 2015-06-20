namespace SabbathText.Tests.V1
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for subscribe messages
    /// </summary>
    [TestClass]
    public class SubscribeMessageTests : TestBase
    {
        /// <summary>
        /// Tests an incoming "subscribe" message after greeting the users
        /// </summary>
        [TestMethod]
        public void SubscribeMessage_SubscribeAfterGreetings()
        {
            AccountEntity account = CreateAccount();
            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            ProcessMessage(subscribeMessage);

            AssertAccountStatus(account.AccountId, AccountStatus.Subscribed);
            AssertConversationContext(account.AccountId, ConversationContext.SubscriptionConfirmed);
            AssertLastSentMessage(account.AccountId, MessageTemplate.PromptZipCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 1);
        }

        /// <summary>
        /// Tests that subsequent calls of the same subscribe message will return an "OperationInProgress" response.
        /// </summary>
        [TestMethod]
        public void SubscribeMessage_OperationInProgress()
        {
            AccountEntity account = CreateAccount();
            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "Subscribe");

            Mock<MessageClient> mockMessageClient = new Mock<MessageClient>();
            mockMessageClient
                .Setup(m => m.SendMessage(It.IsAny<Message>(), It.IsAny<string>() /* trackingId */, It.IsAny<CancellationToken>()))
                .Callback(() => { Trace.TraceInformation("MessageClient.SendMessage mock failure"); })
                .Throws(new ApplicationException("MessageClient.SendMessage mock failure"));

            GoodieBag.CreateFunc = (originalBag) =>
            {
                originalBag.MessageClient = mockMessageClient.Object;
                return originalBag;
            };

            // the first run should result in an error
            OperationResponse<bool> response = ProcessMessage(subscribeMessage);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

            // retrying before compensation will result in "OperationInProgress"
            GoodieBag.CreateFunc = null;
            response = ProcessMessage(subscribeMessage);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            Assert.AreEqual(CommonErrorCodes.OperationInProgress, response.ErrorCode);

            RunCheckpointWorkerAfterCheckpointLock();
            response = ProcessMessage(subscribeMessage);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            mockMessageClient.VerifyAll();
        }
    }
}
