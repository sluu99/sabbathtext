﻿namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;

    /// <summary>
    /// Test cases for the inspect account operation
    /// </summary>
    [TestClass]
    public class InspectAccountTests : TestBase
    {
        /// <summary>
        /// Tests that the operation archives messages
        /// </summary>
        [TestMethod]
        public void InspectAccount_ArchiveMessage()
        {
            GoodieBag bag = GoodieBag.Create();
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));

            string seattle = "98104";
            string orlando = "32803";

            for (int i = 0; i < bag.Settings.RecentMessageThreshold / 4; i++)
            {
                // each iteration will have create 2 incoming messages and 2 outgoing messages
                ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + seattle));
                ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + orlando));
            }

            account = GetAccount(account.AccountId);
            Assert.AreEqual(
                bag.Settings.RecentMessageThreshold + 2,
                account.RecentMessages.Count,
                "The account does not have enough recent messages");

            InspectAccount(account.AccountId);
            account = GetAccount(account.AccountId);
            Assert.AreEqual(
                bag.Settings.RecentMessageThreshold,
                account.RecentMessages.Count,
                "The account should have recent messages at the threshold");
            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscribedForZipCode);

            InMemoryKeyValueStore<MessageEntity> messageStore = (InMemoryKeyValueStore<MessageEntity>)bag.MessageStore;
            Assert.AreEqual(
                2,
                messageStore.Entities.Count,
                "The message store should have 2 archived messages");
        }

        /// <summary>
        /// Ensure that the account always receives unique Bible verses on their Sabbath text
        /// </summary>
        [TestMethod]
        public void InspectAccount_SabbathTextsShouldBeUnique()
        {
            string zipCode = "11434"; // Jamaica, NY (but any ZIP should work)

            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            for (int i = 0; i < 100; i++)
            {
                ClockHelper.GoTo(ClockHelper.GetSabbathStartTime(zipCode));

                InspectAccount(account.AccountId);

                // go to the next day so that GetSabbathStartTime will return next week's Sabbath
                Clock.RollClock(TimeSpan.FromDays(1));
            }

            account = GetAccount(account.AccountId); // get the updated account
            Assert.AreEqual(104, account.RecentMessages.Count);
            Assert.AreEqual(
                account.RecentVerses.Distinct().Count(),
                account.RecentVerses.Count(),
                "Found duplicate texts in recent verses: {0}".InvariantFormat(string.Join("; ", account.RecentVerses)));
        }

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

            InspectAccount(account.AccountId);

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

            InspectAccount(account.AccountId);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscribedForZipCode);
        }

        private void InspectAccount_ShouldSendMessageDuringGracePeriod(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to 15 seconds before grace period ends
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode);
            ClockHelper.GoTo(nextSabbathUtc + GoodieBag.Create().Settings.SabbathTextGracePeriod - TimeSpan.FromSeconds(15));

            InspectAccount(account.AccountId);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
        }
    }
}
