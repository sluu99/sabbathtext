namespace SabbathText.Tests.V1
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for ZIP messages.
    /// </summary>
    [TestClass]
    public class ZipMessageTests : TestBase
    {
        /// <summary>
        /// Tests the first time a user updates the ZIP code after subscribing
        /// </summary>
        [TestMethod]
        public void ZipMessage_FirstTimeZipUpdate()
        {
            const string ZipCode = "60290"; // Chicago
            AccountEntity account = CreateAccount();
            
            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            ProcessMessage(subscribeMessage);

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip " + ZipCode);
            ProcessMessage(zipMessage);

            AssertZipCode(account.AccountId, ZipCode);
            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscribedForZipCode, mustContain: "Chicago");
            AssertMessageCount(account.PhoneNumber, MessageTemplate.SubscribedForZipCode, 1);
        }

        /// <summary>
        /// Tests a ZIP message with a non-existing ZIP code.
        /// </summary>
        [TestMethod]
        public void ZipMessage_LocationNotFound()
        {
            const string ZipCode = "12346";
            AccountEntity account = CreateAccount();

            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            ProcessMessage(subscribeMessage);

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip " + ZipCode + ".");
            ProcessMessage(zipMessage);

            AssertZipCode(account.AccountId, null);
            AssertLastSentMessage(account.AccountId, MessageTemplate.LocationNotFound);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.LocationNotFound, 1);
        }

        /// <summary>
        /// Tests an invalid ZIP message.
        /// </summary>
        [TestMethod]
        public void ZipMessage_InvalidMessage()
        {
            AccountEntity account = CreateAccount();

            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe");
            ProcessMessage(subscribeMessage);

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip blahblahblah!!!");
            ProcessMessage(zipMessage);

            AssertZipCode(account.AccountId, null);
            AssertLastSentMessage(account.AccountId, MessageTemplate.UpdateZipInstruction);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.UpdateZipInstruction, 1);
        }

        /// <summary>
        /// Tests updating the ZIP code multiple times
        /// </summary>
        [TestMethod]
        public void ZipMessage_UpdateMultipleTimes()
        {
            Dictionary<string, string> zipCodes = new Dictionary<string, string>(3)
            {
                { "60290", "Chicago" },
                { "96813", "Honolulu" },
                { "12015", "Athens" },
            };

            AccountEntity account = CreateAccount();

            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            ProcessMessage(subscribeMessage);

            int count = 0;
            foreach (KeyValuePair<string, string> kv in zipCodes)
            {
                count++;
                string zipCode = kv.Key;
                string city = kv.Value;

                Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip " + zipCode);
                ProcessMessage(zipMessage);

                AssertZipCode(account.AccountId, zipCode);
                AssertLastSentMessage(account.AccountId, MessageTemplate.SubscribedForZipCode, mustContain: city);
                AssertMessageCount(account.PhoneNumber, MessageTemplate.SubscribedForZipCode, count);
            }            
        }

        /// <summary>
        /// Tests sending a ZIP message to a brand new account
        /// </summary>
        [TestMethod]
        public void ZipMessage_SubscriptionRequired()
        {
            AccountEntity account = CreateAccount();

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "zip 98052");
            ProcessMessage(zipMessage);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscriptionRequired);
            AssertZipCode(account.AccountId, "98052");
        }

        /// <summary>
        /// Ensures that the account has a certain ZIP code.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedZipCode">The expected ZIP code.</param>
        private static void AssertZipCode(string accountId, string expectedZipCode)
        {
            AccountEntity account =
                GoodieBag.Create().AccountStore.Get(AccountEntity.GetReferenceById(accountId), CancellationToken.None).Result;
            Assert.AreEqual<string>(expectedZipCode, account.ZipCode);
        }
    }
}
