using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabbathText.Core.Entities;
using Moq;
using SabbathText.Core.Backend.InboundProcessors;
using System.Threading.Tasks;

namespace SabbathText.Core.Tests.Backend.InboundProcessors
{
    [TestClass]
    public class ZipCodeProcessorTests
    {
        [TestMethod]
        public void TestUpdateZipOneHourAfterSabbathStarted()
        {
            Message updateZipMessage = new Message
            {
                Body = "Zip 00000",
                CreationTime = Clock.UtcNow,
                MessageId = Guid.NewGuid().ToString(),
                Sender = "+14230010001",                
            };

            Account acct = new Account()
            {
                AccountId = Guid.NewGuid().ToString(),
                CreationTime = Clock.UtcNow,
                PhoneNumber = "+14230010001",
                Status = AccountStatus.Subscribed,
                ZipCode = "00000",
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

            ZipCodeProcessor processor = new ZipCodeProcessor
            {
                DataProvider = dataProviderMock.Object,
            };

            TemplatedMessage response = processor.ProcessMessage(updateZipMessage).Result;
            Assert.AreEqual(MessageTemplate.ConfirmZipCodeUpdate, response.Template);
        }
    }
}
