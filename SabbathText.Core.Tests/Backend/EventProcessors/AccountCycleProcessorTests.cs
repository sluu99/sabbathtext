using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;
using SabbathText.Core.Entities;
using SabbathText.Core.Backend.EventProcessors;

namespace SabbathText.Core.Tests.Backend.EventProcessors
{
    [TestClass]
    public class AccountCycleProcessorTests
    {
        [TestMethod]
        public void TestAccountCycleWakesUpOnSabbath()
        {
            using (FakeClockScope scope = new FakeClockScope())
            {
                Clock.ResetClock();
                Clock.UseFakeClock();

                Account acct = new Account()
                {
                    AccountId = Guid.NewGuid().ToString(),
                    CreationTime = Clock.UtcNow,
                    PhoneNumber = "+14230010001",
                    Status = AccountStatus.Subscribed,
                    ZipCode = "00000",
                    LastSabbathMessageTime = Clock.UtcNow.AddDays(-7),
                };

                Location location = new Location()
                {
                    ZipCode = "00000",
                    TimeZoneOffset = 0,
                    TimeZoneName = "UTC",
                    City = "Test",
                    State = "AAA",
                    Country = "USA",
                };

                LocationTimeInfo timeInfo = new LocationTimeInfo()
                {
                    Sunset = Clock.UtcNow.AddHours(-1),
                };

                Mock<IDataProvider> dataProviderMock = new Mock<IDataProvider>();
                dataProviderMock.Setup(x => x.GetAccountByPhoneNumber(acct.PhoneNumber)).Returns(Task.FromResult(acct));
                dataProviderMock.Setup(x => x.GetLocationByZipCode(location.ZipCode)).Returns(Task.FromResult(location));
                dataProviderMock.Setup(x => x.GetTimeInfoByZipCode(location.ZipCode, It.IsAny<DateTime>())).Returns(Task.FromResult(timeInfo));

                AccountCycleProcessor processor = new AccountCycleProcessor
                {
                    DataProvider = dataProviderMock.Object,
                };

                Message accountCycleMessage = new Message
                {
                    Body = "AccountCycle",
                    CreationTime = Clock.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Sender = "+14230010001",
                };

                TemplatedMessage response = processor.ProcessMessage(accountCycleMessage).Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageTemplate.HappySabbath, response.Template);
            }
        }
    }
}
