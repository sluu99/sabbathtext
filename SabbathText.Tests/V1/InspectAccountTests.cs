namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;

    /// <summary>
    /// Test cases for the inspect account operation
    /// </summary>
    [TestClass]
    public class InspectAccountTests : TestBase
    {
        /// <summary>
        /// Tests that the operation sends out Sabbath message for Redmond
        /// </summary>
        [TestMethod]
        public void InspectAccount_SendMessageOnSabbath_Redmond()
        {
            this.InspectAccount_SendMessageOnSabbath("98052");
        }

        /// <summary>
        /// Tests that the operation sends out Sabbath message for DC
        /// </summary>
        [TestMethod]
        public void InspectAccount_SendMessageOnSabbath_DC()
        {
            this.InspectAccount_SendMessageOnSabbath("20001");
        }

        /// <summary>
        /// Tests that the operation sends out Sabbath message for Dallas
        /// </summary>
        [TestMethod]
        public void InspectAccount_SendMessageOnSabbath_Dallas()
        {
            this.InspectAccount_SendMessageOnSabbath("75203");
        }

        /// <summary>
        /// Tests that the operation does not send out Sabbath message before sun down
        /// </summary>
        [TestMethod]
        public void InspectAccount_ShouldNotSendMessageBeforeSabbath_Atlanta()
        {
            this.InspectAccount_ShouldNotSendMessageBeforeSabbath("30369");
        }

        /// <summary>
        /// Tests that the operation does not send out Sabbath message before sun down
        /// </summary>
        [TestMethod]
        public void InspectAccount_ShouldNotSendMessageBeforeSabbath_BeverlyHills()
        {
            this.InspectAccount_ShouldNotSendMessageBeforeSabbath("90210");
        }

        /// <summary>
        /// Tests that the operation still sends out Sabbath message until the end of grace period
        /// </summary>
        [TestMethod]
        public void InspectAccount_ShouldSendMessageDuringGracePeriod_Boston()
        {
            this.InspectAccount_ShouldSendMessageDuringGracePeriod("02110");
        }
        
        private void InspectAccount_SendMessageOnSabbath(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to the next Sabbath
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode);
            ClockHelper.GoTo(nextSabbathUtc + TimeSpan.FromSeconds(1));

            InspectAccount(account);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
        }

        private void InspectAccount_ShouldNotSendMessageBeforeSabbath(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to 15 seconds before Sabbath
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode);
            ClockHelper.GoTo(nextSabbathUtc - TimeSpan.FromSeconds(15));

            InspectAccount(account);

            AssertLastSentMessage(account.AccountId, MessageTemplate.ZipCodeUpdated);
        }

        private void InspectAccount_ShouldSendMessageDuringGracePeriod(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));
            
            // go to 15 seconds before grace period ends
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode);
            ClockHelper.GoTo(nextSabbathUtc + GoodieBag.Create().Settings.SabbathTextGracePeriod - TimeSpan.FromSeconds(15));

            InspectAccount(account);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
        }
    }
}
