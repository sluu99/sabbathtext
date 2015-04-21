namespace SabbathText.Tests.V1
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for scenarios which the the conversation context is "Greetings"
    /// </summary>
    [TestClass]
    public class SubscribeMessageTests : ProcessMessageOperationTests
    {
        /// <summary>
        /// Tests an incoming "subscribe" message after greeting the users
        /// </summary>
        [TestMethod]
        public void SubscribeAfterGreetings()
        {
            AccountEntity account = this.CreateAccount();
            this.GreetUser(account);
            this.AssertConversationContext(account.AccountId, ConversationContext.Greetings);

            Message subscribeMessage = this.CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            this.ProcessMessage(account, subscribeMessage);
            this.AssertConversationContext(account.AccountId, ConversationContext.SubscriptionConfirmed);
            this.AssertLastSentMessage(account.AccountId, MessageTemplate.SubscriptionConfirmed);
        }
    }
}
