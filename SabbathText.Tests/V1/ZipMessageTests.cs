namespace SabbathText.Tests.V1
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for ZIP messages.
    /// </summary>
    [TestClass]
    public class ZipMessageTests : ProcessMessageOperationTests
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
            ProcessMessage(account.AccountId, subscribeMessage);

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip " + ZipCode);
            ProcessMessage(account.AccountId, zipMessage);

            AssertZipCode(account.AccountId, ZipCode);
            AssertLastSentMessage(account.AccountId, MessageTemplate.ZipCodeUpdated, mustContain: "Chicago");
            AssertMessageCount(account.PhoneNumber, MessageTemplate.ZipCodeUpdated, 1);
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
            ProcessMessage(account.AccountId, subscribeMessage);

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip " + ZipCode + ".");
            ProcessMessage(account.AccountId, zipMessage);

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
            ProcessMessage(account.AccountId, subscribeMessage);

            Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip blahblahblah!!!");
            ProcessMessage(account.AccountId, zipMessage);

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
            ProcessMessage(account.AccountId, subscribeMessage);

            int count = 0;
            foreach (KeyValuePair<string, string> kv in zipCodes)
            {
                count++;
                string zipCode = kv.Key;
                string city = kv.Value;

                Message zipMessage = CreateIncomingMessage(account.PhoneNumber, "Zip " + zipCode);
                ProcessMessage(account.AccountId, zipMessage);

                AssertZipCode(account.AccountId, zipCode);
                AssertLastSentMessage(account.AccountId, MessageTemplate.ZipCodeUpdated, mustContain: city);
                AssertMessageCount(account.PhoneNumber, MessageTemplate.ZipCodeUpdated, count);
            }            
        }
    }
}
