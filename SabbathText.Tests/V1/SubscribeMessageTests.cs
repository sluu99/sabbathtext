namespace SabbathText.Tests.V1
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for subscribe messages
    /// </summary>
    [TestClass]
    public class SubscribeMessageTests : TestBase
    {
        /// <summary>
        /// Tests an incoming "subscribe" message after greeting the users
        /// </summary>
        [TestMethod]
        public void SubscribeMessage_SubscribeAfterGreetings()
        {
            AccountEntity account = CreateAccount();
            GreetUser(account);
            AssertConversationContext(account.AccountId, ConversationContext.Greetings);
            AssertAccountStatus(account.AccountId, AccountStatus.BrandNew);

            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            ProcessMessage(subscribeMessage);

            AssertAccountStatus(account.AccountId, AccountStatus.Subscribed);
            AssertConversationContext(account.AccountId, ConversationContext.SubscriptionConfirmed);
            AssertLastSentMessage(account.AccountId, MessageTemplate.PromptZipCode);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.PromptZipCode, 1);
        }
    }
}
