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
            AssertLastSentMessage(account.AccountId, MessageTemplate.ZipCodeUpdated);
        }
    }
}
