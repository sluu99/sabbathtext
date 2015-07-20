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
            AssertLastSentMessage(account.AccountId, MessageTemplate.PromptZipCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 1);
        }

        /// <summary>
        /// Tests subscribe message compensation logic.
        /// </summary>
        [TestMethod]
        public void SubscribeMessage_Compensation()
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
            GoodieBag.CreateFunc = null;
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            AssertAccountStatus(account.AccountId, AccountStatus.BrandNew);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 0);

            // retrying before compensation will result in "OperationInProgress"            
            response = ProcessMessage(subscribeMessage);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            Assert.AreEqual(CommonErrorCodes.OperationInProgress, response.ErrorCode);
            AssertAccountStatus(account.AccountId, AccountStatus.BrandNew);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 0);

            // run compensation
            RunCheckpointWorkerAfterCheckpointLock();

            // retrying now will return a successful response
            response = ProcessMessage(subscribeMessage);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            AssertAccountStatus(account.AccountId, AccountStatus.Subscribed);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 1);
            AssertLastSentMessage(account.AccountId, MessageTemplate.PromptZipCode);

            // retrying will not process the message again
            // so it should not hit the error
            GoodieBag.CreateFunc = (originalBag) =>
            {
                originalBag.MessageClient = mockMessageClient.Object;
                return originalBag;
            };
            response = ProcessMessage(subscribeMessage);
            GoodieBag.CreateFunc = null;

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            AssertAccountStatus(account.AccountId, AccountStatus.Subscribed);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 1);
            AssertLastSentMessage(account.AccountId, MessageTemplate.PromptZipCode);

            mockMessageClient.VerifyAll();
        }

        /// <summary>
        /// Tests that sending another subscribe message after the user has provided the ZIP code
        /// will not affect the account.
        /// </summary>
        [TestMethod]
        public void SubscribeMessage_SubscribeAfterZipCode()
        {
            const string ZipCode = "71940";

            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + ZipCode));

            AssertAccountStatus(account.AccountId, AccountStatus.Subscribed);
            account = GetAccount(account.AccountId);
            Assert.AreEqual(ZipCode, account.ZipCode, "Initial ZIP code validation failed");
        
            // Send another subscribe message
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe!!!"));
            AssertAccountStatus(account.AccountId, AccountStatus.Subscribed);
            account = GetAccount(account.AccountId);
            Assert.AreEqual(ZipCode, account.ZipCode, "ZIP code should not have changed");
        }
    }
}
