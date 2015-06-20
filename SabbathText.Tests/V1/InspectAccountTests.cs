namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.Location.V1;

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
            // go to a non-Sabbath day to make sure InspectAccount does not send out any messages
            ClockHelper.GoToDay(DayOfWeek.Wednesday);

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
                DateTime sabbathEndsUtc;
                ClockHelper.GoTo(ClockHelper.GetSabbathStartTime(zipCode, out sabbathEndsUtc));

                InspectAccount(account.AccountId);

                // go to the next day so that GetSabbathStartTime will return next week's Sabbath
                Clock.Delay(TimeSpan.FromDays(1)).Wait();
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
        /// Tests that running inspect account multiple times will not send out multiple Sabbath texts
        /// </summary>
        [TestMethod]
        public void InspectAccount_MultipleRunsOnlySendsSabbathTextOnce_Nashville()
        {
            this.InspectAccount_MultipleRunsOnlySendsSabbathTextOnce("37115");
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
        public void InspectAccount_ShouldSendMessageUntilSabbathEnds_Boston()
        {
            this.InspectAccount_ShouldSendMessageUntilSabbathEnds("02110");
        }

        /// <summary>
        /// Tests that inspecting the account will schedule sending out a Sabbath message.
        /// </summary>
        [TestMethod]
        public void InspectAccount_ShouldScheduleSabbathMessageOperation_Houston()
        {
            this.InspectAccount_ShouldScheduleSabbathMessageOperation("77318");
        }

        /// <summary>
        /// Tests that inspect account sends out announcements
        /// </summary>
        [TestMethod]
        public void InspectAccount_SendAnnouncements()
        {
            string zipCode = "80123"; // Denver, CO
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            Clock.Delay(TimeSpan.FromDays(10)).Wait();
            ClockHelper.GoToDay(DayOfWeek.Tuesday); // go to some time that's not the Sabbath

            LocationInfo locationInfo = LocationInfo.FromZipCode(zipCode);
            TimeInfo timeInfo = TimeInfo.Create(zipCode, locationInfo.LocalTime.Date);

            DateTime timeToSendAnnouncement = timeInfo.SunSetUtc - TimeSpan.FromHours(4.5);
            if (timeToSendAnnouncement < Clock.UtcNow)
            {
                // we have passed the announcement time, go to the next day
                TimeInfo nextDayTimeInfo = TimeInfo.Create(zipCode, locationInfo.LocalTime.Date.AddDays(1));
                timeToSendAnnouncement = nextDayTimeInfo.SunSetUtc - TimeSpan.FromHours(4.5);
            }
                        
            // go to announcement time
            ClockHelper.GoTo(timeToSendAnnouncement);

            InspectAccount(account.AccountId);
            AssertLastSentMessage(account.AccountId, MessageTemplate.Announcement);
        }

        private void InspectAccount_ShouldScheduleSabbathMessageOperation(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // suppose the next InspectAccount run will 1 second after Sabbath started
            DateTime sabbathEndsUtc;
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode, out sabbathEndsUtc);
            ClockHelper.GoTo(nextSabbathUtc.Subtract(new TestEnvironmentSettings().RunnerFrequency).AddSeconds(1));

            InspectAccount(account.AccountId);

            // make sure we have not sent out any Sabbath messages
            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscribedForZipCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.SabbathText, 0);

            // go to the Sabbath and run the checkpoint worker to send out a Sabbath message
            ClockHelper.GoTo(nextSabbathUtc);
            RunCheckpointWorker();
            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.SabbathText, 1);
        }

        private void InspectAccount_MultipleRunsOnlySendsSabbathTextOnce(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to the next Sabbath
            DateTime sabbathEndsUtc;
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode, out sabbathEndsUtc);
            ClockHelper.GoTo(nextSabbathUtc.AddSeconds(1));

            for (int i = 0; i < 10; i++)
            {
                InspectAccount(account.AccountId);
                Clock.Delay(new TestEnvironmentSettings().RunnerFrequency).Wait();
            }

            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.SabbathText, 1);
        }

        private void InspectAccount_SendMessageOnSabbath(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to the next Sabbath
            DateTime sabbathEndsUtc;
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode, out sabbathEndsUtc);
            ClockHelper.GoTo(nextSabbathUtc.AddSeconds(1));

            InspectAccount(account.AccountId);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
        }

        private void InspectAccount_ShouldNotSendMessageBeforeSabbath(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to 15 seconds before Sabbath
            DateTime sabbathEndsUtc;
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode, out sabbathEndsUtc);
            ClockHelper.GoTo(nextSabbathUtc.AddSeconds(-15));

            InspectAccount(account.AccountId);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscribedForZipCode);
        }

        private void InspectAccount_ShouldSendMessageUntilSabbathEnds(string zipCode)
        {
            AccountEntity account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "zip " + zipCode));

            // go to 15 seconds before Sabbath ends
            DateTime sabbathEndsUtc;
            DateTime nextSabbathUtc = ClockHelper.GetSabbathStartTime(zipCode, out sabbathEndsUtc);
            ClockHelper.GoTo(sabbathEndsUtc.AddSeconds(-15));

            InspectAccount(account.AccountId);

            AssertLastSentMessage(account.AccountId, MessageTemplate.SabbathText);
        }
    }
}
