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
    /// This class tests the Hello messages
    /// </summary>
    [TestClass]
    public class HelloMessageTests : TestBase
    {
        /// <summary>
        /// Tests the response of the operation when the account is brand new.
        /// </summary>
        [TestMethod]
        public void HelloMessage_BrandNewAccount()
        {
            var account = CreateAccount();
            Assert.AreEqual(AccountStatus.BrandNew, account.Status);

            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "hello!"));
            AssertLastSentMessage(account.AccountId, MessageTemplate.Greetings);
        }

        /// <summary>
        /// Tests the response of the operation when the account has subscribed to the service!
        /// </summary>
        [TestMethod]
        public void HelloMessage_Subscriber()
        {
            var account = CreateAccount();
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "subscribe"));
            ProcessMessage(CreateIncomingMessage(account.PhoneNumber, "hi"));

            AssertLastSentMessage(account.AccountId, MessageTemplate.CommandList);
        }
    }
}
