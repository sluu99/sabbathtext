namespace SabbathText.Tests.V1
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
