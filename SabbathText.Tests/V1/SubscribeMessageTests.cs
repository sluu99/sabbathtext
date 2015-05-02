﻿namespace SabbathText.Tests.V1
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for subscribe messages
    /// </summary>
    [TestClass]
    public class SubscribeMessageTests : ProcessMessageOperationTests
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

            Message subscribeMessage = CreateIncomingMessage(account.PhoneNumber, "subscribe!!");
            ProcessMessage(account.AccountId, subscribeMessage);
            AssertConversationContext(account.AccountId, ConversationContext.SubscriptionConfirmed);
            AssertLastSentMessage(account.AccountId, MessageTemplate.SubscriptionConfirmed);
            AssertMessageCount(account.PhoneNumber, MessageTemplate.SubscriptionConfirmed, 1);
        }
    }
}