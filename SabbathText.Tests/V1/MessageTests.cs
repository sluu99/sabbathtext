namespace SabbathText.Tests.V1
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.V1;

    /// <summary>
    /// Contains test cases for messages
    /// </summary>
    [TestClass]
    public class MessageTests
    {
        private const int MaxTextLength = 160;

        /// <summary>
        /// Tests the length of the Sabbath Text content
        /// </summary>
        [TestMethod]
        public void Message_SabbathTextLength()
        {
            foreach (var verse in DomainData.BibleVerses)
            {
                string messageBody = Message.CreateSabbathText("+11234567890", verse.Key, verse.Value).Body;

                Assert.IsTrue(messageBody.Length > 0, "Message length must be > 0");
                Assert.IsTrue(messageBody.Length <= MaxTextLength, "Message length exceeded {0} for {1}".InvariantFormat(MaxTextLength, verse.Key));
                StringAssert.Contains(messageBody, verse.Key);
                StringAssert.Contains(messageBody, verse.Value);
            }
        }
    }
}
