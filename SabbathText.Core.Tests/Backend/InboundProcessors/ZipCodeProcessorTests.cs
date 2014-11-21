using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabbathText.Core.Entities;
using Moq;
using SabbathText.Core.Backend.InboundProcessors;
using System.Threading.Tasks;
using SabbathText.Core.Backend;

namespace SabbathText.Core.Tests.Backend.InboundProcessors
{
    [TestClass]
    public class ZipCodeProcessorTests
    {
        [TestMethod]
        public void TestUpdateZipOneHourAfterSabbathStarted()
        {
            Account acct = TestHelper.GenerateAccount();
                        
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

            Message updateZipMessage = TestHelper.CreateInboundMessage(acct.PhoneNumber, "Zip " + location.ZipCode);

            ZipCodeProcessor processor = MessageRouter.NewInboundRouter().GetProcessor(updateZipMessage) as ZipCodeProcessor;
            processor.DataProvider = dataProviderMock.Object;
            
            TemplatedMessage response = processor.ProcessMessage(updateZipMessage).Result;
            Assert.AreEqual(MessageTemplate.ConfirmZipCodeUpdate, response.Template);
        }
    }
}
