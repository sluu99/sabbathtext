namespace SabbathText.Location.Tests.V1
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Location.V1;

    /// <summary>
    /// Test cases for LocationInfo
    /// </summary>
    [TestClass]
    public class LocationInfoTests
    {
        private FakeClockScope fakeClockScope;

        /// <summary>
        /// Initializations before each test case
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            this.fakeClockScope = new FakeClockScope();
        }

        /// <summary>
        /// Cleans up after each test case
        /// </summary>
        [TestCleanup]
        public void TestCleanUp()
        {
            this.fakeClockScope.Dispose();
        }

        /// <summary>
        /// Test Redmond, WA LocationInfo
        /// </summary>
        [TestMethod]
        public void TestLocationInformation_RedmondWA()
        {
            Clock.Freeze(new DateTime(2015, 5, 23, 19, 20, 13, DateTimeKind.Utc));

            LocationInfo redmond = LocationInfo.FromZipCode("98052");
            Assert.AreEqual("98052", redmond.ZipCode);
            Assert.AreEqual(LocationType.Standard, redmond.Type);
            Assert.AreEqual("Redmond", redmond.PrimaryCity);
            Assert.AreEqual("WA", redmond.State);
            Assert.AreEqual("US", redmond.Country);
            Assert.AreEqual("America/Los_Angeles", redmond.TimeZoneName);
            Assert.IsTrue(
                Math.Abs(redmond.Latitude - 47.68) < double.Epsilon,
                string.Format("Expected latitude: {0}. Actual latitude: {1}", 47.68, redmond.Latitude));
            Assert.IsTrue(
                Math.Abs(redmond.Longitude - (-122.12)) < double.Epsilon,
                string.Format("Expected longitude: {0}. Actual longitude: {1}", -122.12, redmond.Latitude));
            Assert.AreEqual(
                new DateTime(2015, 5, 23, 12, 20, 13, DateTimeKind.Unspecified),
                redmond.LocalTime);
        }

        /// <summary>
        /// Test Collegedale, TN LocationInfo
        /// </summary>
        [TestMethod]
        public void TestLocationInformation_CollegedaleTN()
        {
            Clock.Freeze(new DateTime(2015, 5, 23, 19, 20, 13, DateTimeKind.Utc));

            LocationInfo collegedale = LocationInfo.FromZipCode("37315");
            Assert.AreEqual("37315", collegedale.ZipCode);
            Assert.AreEqual(LocationType.POBox, collegedale.Type);
            Assert.AreEqual("Collegedale", collegedale.PrimaryCity);
            Assert.AreEqual("TN", collegedale.State);
            Assert.AreEqual("US", collegedale.Country);
            Assert.AreEqual("America/New_York", collegedale.TimeZoneName);
            Assert.IsTrue(
                Math.Abs(collegedale.Latitude - 35.04) < double.Epsilon,
                string.Format("Expected latitude: {0}. Actual latitude: {1}", 35.04, collegedale.Latitude));
            Assert.IsTrue(
                Math.Abs(collegedale.Longitude - (-85.05)) < double.Epsilon,
                string.Format("Expected longitude: {0}. Actual longitude: {1}", -85.05, collegedale.Latitude));
            Assert.AreEqual(
                new DateTime(2015, 5, 23, 15, 20, 13, DateTimeKind.Unspecified),
                collegedale.LocalTime);
        }

        /// <summary>
        /// Test Verona, WI LocationInfo
        /// </summary>
        [TestMethod]
        public void TestLocationInformation_VeronaWI()
        {
            Clock.Freeze(new DateTime(2015, 5, 23, 19, 20, 13, DateTimeKind.Utc));

            LocationInfo verona = LocationInfo.FromZipCode("53593");
            Assert.AreEqual("53593", verona.ZipCode);
            Assert.AreEqual(LocationType.Standard, verona.Type);
            Assert.AreEqual("Verona", verona.PrimaryCity);
            Assert.AreEqual("WI", verona.State);
            Assert.AreEqual("US", verona.Country);
            Assert.AreEqual("America/Chicago", verona.TimeZoneName);
            Assert.IsTrue(
                Math.Abs(verona.Latitude - 42.98) < double.Epsilon,
                string.Format("Expected latitude: {0}. Actual latitude: {1}", 42.98, verona.Latitude));
            Assert.IsTrue(
                Math.Abs(verona.Longitude - (-89.53)) < double.Epsilon,
                string.Format("Expected longitude: {0}. Actual longitude: {1}", -89.53, verona.Latitude));
            Assert.AreEqual(
                new DateTime(2015, 5, 23, 14, 20, 13, DateTimeKind.Unspecified),
                verona.LocalTime);
        }
    }
}
