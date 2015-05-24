namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Authentication test cases
    /// </summary>
    [TestClass]
    public class AuthenticationTests : TestBase
    {
        /// <summary>
        /// Tests begin authentication success scenario
        /// </summary>
        [TestMethod]
        public void BeginAuth_Success()
        {
            try
            {
                string authKey = Guid.NewGuid().ToString();

                AuthKey.CreateFunc = () => authKey;

                AccountEntity account = CreateAccount();
                Message message = CreateIncomingMessage(account.PhoneNumber, "Auth.");
                ProcessMessage(message);

                AssertAccountStatus(account.AccountId, AccountStatus.BrandNew);
                AssertLastSentMessage(account.AccountId, MessageTemplate.AuthKeyCreated);
                AssertAuthKey(account.AccountId, authKey);
            }
            finally
            {
                AuthKey.CreateFunc = null;
            }
        }

        /// <summary>
        /// Asserts that the account has the expected authentication key.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="expectedAuthKey">The expected authentication key.</param>
        protected static void AssertAuthKey(string accountId, string expectedAuthKey)
        {
            AccountEntity account =
                GoodieBag.Create().AccountStore.Get(AccountEntity.GetReferenceById(accountId), CancellationToken.None).Result;
            Assert.AreEqual<string>(expectedAuthKey, account.AuthKey);
        }
    }
}
