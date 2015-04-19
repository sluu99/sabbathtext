namespace SabbathText.Location.Tests.V1
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Location.V1;

    /// <summary>
    /// Test cases for TimeInfo
    /// </summary>
    [TestClass]
    public class TimeInfoTests
    {
        /// <summary>
        /// Test Redmond, WA TimeInfo
        /// </summary>
        [TestMethod]
        public void TestTimeInformation_RedmondWA()
        {
            TimeInfo timeInfo = TimeInfo.Create(
                "98052",
                new DateTime(2015, 4, 19));
            Assert.AreEqual("98052", timeInfo.ZipCode);
            Assert.AreEqual(new DateTime(2015, 4, 19), timeInfo.TimeInfoDate);
            Assert.AreEqual(
                new DateTime(2015, 4, 19, 20, 2, 11, 188, DateTimeKind.Unspecified),
                timeInfo.SunSetDestination);
            Assert.AreEqual(
                new DateTime(2015, 4, 20, 3, 2, 11, 188, DateTimeKind.Utc),
                timeInfo.SunSetUtc);
        }

        /// <summary>
        /// Test Collegedale, TN TimeInfo
        /// </summary>
        [TestMethod]
        public void TestTimeInformation_CollegedaleTN()
        {
            TimeInfo timeInfo = TimeInfo.Create(
                "37315",
                new DateTime(2015, 4, 19));
            Assert.AreEqual("37315", timeInfo.ZipCode);
            Assert.AreEqual(new DateTime(2015, 4, 19), timeInfo.TimeInfoDate);
            Assert.AreEqual(
                new DateTime(2015, 4, 19, 20, 14, 58, 876, DateTimeKind.Unspecified),
                timeInfo.SunSetDestination);
            Assert.AreEqual(
                new DateTime(2015, 4, 20, 0, 14, 58, 876, DateTimeKind.Utc),
                timeInfo.SunSetUtc);
        }

        /// <summary>
        /// Test Verona, WI TimeInfo
        /// </summary>
        [TestMethod]
        public void TestTimeInformation_VeronaWI()
        {
            TimeInfo timeInfo = TimeInfo.Create(
                "53593",
                new DateTime(2015, 4, 19));
            Assert.AreEqual("53593", timeInfo.ZipCode);
            Assert.AreEqual(new DateTime(2015, 4, 19), timeInfo.TimeInfoDate);
            Assert.AreEqual(
                new DateTime(2015, 4, 19, 19, 43, 49, 822, DateTimeKind.Unspecified),
                timeInfo.SunSetDestination);
            Assert.AreEqual(
                new DateTime(2015, 4, 20, 0, 43, 49, 822, DateTimeKind.Utc),
                timeInfo.SunSetUtc);
        }
    }
}
