namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for the Bible verse message texts
    /// </summary>
    [TestClass]
    public class BibleVerseMessageTests : TestBase
    {
        /// <summary>
        /// Tests when the user requests for a Bible verse
        /// </summary>
        [TestMethod]
        public void BibleVerseMessage_Success()
        {
            AccountEntity account = CreateAccount();

            Message bibleVerseMessage = CreateIncomingMessage(account.PhoneNumber, "bible verse.");
            ProcessMessage(bibleVerseMessage);

            account = GetAccount(account.AccountId);
            Assert.AreEqual(1, account.RecentVerses.Count, "Recent verses count mismatch");

            AssertLastSentMessage(account.AccountId, MessageTemplate.BibleVerse, mustContain: account.RecentVerses[0]);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.BibleVerse, 1);
        }

        /// <summary>
        /// Tests that the Bible verses sent out are unique
        /// </summary>
        [TestMethod]
        public void BibleVerseMessage_VersesShouldBeUnique()
        {
            AccountEntity account = CreateAccount();

            for (int i = 0; i < 100; i++)
            {
                Message incomingMessage = CreateIncomingMessage(account.PhoneNumber, "verse");
                ProcessMessage(incomingMessage);
            }

            account = GetAccount(account.AccountId);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.BibleVerse, 100);
            Assert.AreEqual(
                account.RecentVerses.Distinct().Count(),
                account.RecentVerses.Count,
                "Bible verses are not distinct");
        }

        /// <summary>
        /// Tests compensation logic for "Bible verse" incoming messages
        /// </summary>
        [TestMethod]
        public void BibleVerseMessage_Compensation()
        {
            AccountEntity account = CreateAccount();
            Message bibleVerseMessage = CreateIncomingMessage(account.PhoneNumber, "Bible verse");

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
            OperationResponse<bool> response = ProcessMessage(bibleVerseMessage, HttpStatusCode.InternalServerError);
            GoodieBag.CreateFunc = null;
            AssertMessageCount(account.PhoneNumber, MessageTemplate.BibleVerse, 0);

            // retrying before compensation will result in "OperationInProgress"            
            response = ProcessMessage(bibleVerseMessage, HttpStatusCode.Conflict);
            Assert.AreEqual(CommonErrorCodes.OperationInProgress, response.ErrorCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.BibleVerse, 0);

            // run compensation
            RunCheckpointWorkerAfterCheckpointLock();

            // retrying now will return a successful response
            response = ProcessMessage(bibleVerseMessage);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.BibleVerse, 1);
            AssertLastSentMessage(account.AccountId, MessageTemplate.BibleVerse);

            // retrying will not process the message again
            // so it should not hit the error
            GoodieBag.CreateFunc = (originalBag) =>
            {
                originalBag.MessageClient = mockMessageClient.Object;
                return originalBag;
            };
            response = ProcessMessage(bibleVerseMessage);
            GoodieBag.CreateFunc = null;

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.BibleVerse, 1);
            AssertLastSentMessage(account.AccountId, MessageTemplate.BibleVerse);

            mockMessageClient.VerifyAll();
        }
    }
}
