namespace SabbathText.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test cases for the announcement domain data
    /// </summary>
    [TestClass]
    public class DomainDataAnnouncementTests
    {
        /// <summary>
        /// Test that the announcement IDs are unique
        /// </summary>
        [TestMethod]
        public void DomainData_Annoucements_UniqueID()
        {
            var allAnnouncementIds = DomainData.Announcements.Select(a => a.AnnouncementId);
            var uniqueIds = allAnnouncementIds.Distinct();

            Assert.AreEqual(uniqueIds.Count(), allAnnouncementIds.Count());
        }
    }
}
