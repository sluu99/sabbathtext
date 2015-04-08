﻿namespace SabbathText.Tests.V1
{
    using System;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Test cases for ProcessMessageOperation
    /// </summary>
    [TestClass]
    public class ProcessMessageOperationTests : TestBase
    {
        /// <summary>
        /// Creates an incoming message.
        /// </summary>
        /// <param name="sender">The sender phone number.</param>
        /// <param name="body">The message body.</param>
        /// <returns>The message.</returns>
        protected Message CreateIncomingMessage(string sender, string body)
        {
            return new Message
            {
               Body = body,
               ExternalId = Guid.NewGuid().ToString(),
               Recipient = null,
               Sender = sender,
               Template = MessageTemplate.FreeForm,
               Timestamp = Clock.UtcNow,
            };
        }

        /// <summary>
        /// Sends a greeting message to a specific account.
        /// </summary>
        /// <param name="account">The account.</param>
        protected void GreetUser(AccountEntity account)
        {
            OperationContext context = this.CreateContext(account);
            GreetUserOperation operation = new GreetUserOperation(context);
            OperationResponse<bool> response = operation.Run().Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.Accepted, response.StatusCode);
            this.AssertOperationFinishes(context);
        }

        /// <summary>
        /// Processes an incoming message.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="incomingMessage">The incoming message.</param>
        protected void ProcessMessage(AccountEntity account, Message incomingMessage)
        {
            OperationContext context = this.CreateContext(account);
            ProcessMessageOperation operation = new ProcessMessageOperation(context);
            OperationResponse<bool> response = operation.Run(incomingMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.Accepted, response.StatusCode);
            this.AssertOperationFinishes(context);
        }
    }
}
