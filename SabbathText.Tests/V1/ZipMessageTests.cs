namespace SabbathText.Tests.V1
{
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
            
            Message subscribeMessage = this.CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            this.ProcessMessage(account, subscribeMessage);

            Message zipMessage = this.CreateIncomingMessage(account.PhoneNumber, "Zip " + ZipCode);
            this.ProcessMessage(account, zipMessage);

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

            Message subscribeMessage = this.CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            this.ProcessMessage(account, subscribeMessage);

            Message zipMessage = this.CreateIncomingMessage(account.PhoneNumber, "Zip " + ZipCode + ".");
            this.ProcessMessage(account, zipMessage);

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

            Message subscribeMessage = this.CreateIncomingMessage(account.PhoneNumber, "subscribe");
            this.ProcessMessage(account, subscribeMessage);

            Message zipMessage = this.CreateIncomingMessage(account.PhoneNumber, "Zip blahblahblah!!!");
            this.ProcessMessage(account, zipMessage);

            AssertZipCode(account.AccountId, null);
            AssertLastSentMessage(account.AccountId, MessageTemplate.UpdateZipInstruction);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.UpdateZipInstruction, 1);
        }
    }
}
