namespace QueueStorage.Tests
{
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This class tests the QueueStore class
    /// </summary>
    [TestClass]
    public class QueueStoreTests
    {
        /// <summary>
        /// Gets or sets the queue store used for testing
        /// </summary>
        protected QueueStore Store { get; set; }

        /// <summary>
        /// This method will be called before every test run
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.InitStore();
        }

        /// <summary>
        /// AddMessage should allow messages with the same body
        /// </summary>
        [TestMethod]
        public void AddMessage_ShouldAllowDuplicates()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            // call get message twice to make sure that they are two different messages
            QueueMessage msg1 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromMinutes(5), cancellationToken: CancellationToken.None).Result;
            QueueMessage msg2 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromMinutes(5), cancellationToken: CancellationToken.None).Result;

            Assert.AreNotEqual(msg1.MessageId, msg2.MessageId, "The two messages should not have the same ID");
            Assert.AreEqual(1, msg1.DequeueCount, "Message1 should have dequeue count = 1");
            Assert.AreEqual(1, msg2.DequeueCount, "Message2 should have dequeue count = 1");
        }
        
        /// <summary>
        /// Initializes the queue store used for testing
        /// </summary>
        protected virtual void InitStore()
        {
            this.Store = new QueueStore();
            this.Store.InitMemory();
        }

        /// <summary>
        /// Clean up the store after testing
        /// </summary>
        protected virtual void CleanUpStore()
        {            
        }
    }
}
