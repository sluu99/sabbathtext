namespace QueueStorage.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
        protected InMemoryQueueStore Store { get; set; }

        /// <summary>
        /// This method will be called before every test run
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.InitStore();
        }

        /// <summary>
        /// this method will be called after test runs
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            this.CleanUpStore();
        }

        /// <summary>
        /// AddMessage should allow messages with the same body
        /// </summary>
        [TestMethod]
        public void AddMessage_ShouldAllowDuplicateBody()
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

            // make sure that they are two different messages
            QueueMessage msg1 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromMinutes(5), cancellationToken: CancellationToken.None).Result;
            QueueMessage msg2 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromMinutes(5), cancellationToken: CancellationToken.None).Result;

            Assert.AreNotEqual(msg1.MessageId, msg2.MessageId, "The two messages should not have the same ID");
            Assert.AreEqual(1, msg1.DequeueCount, "Message1 should have dequeue count = 1");
            Assert.AreEqual(1, msg2.DequeueCount, "Message2 should have dequeue count = 1");
        }

        /// <summary>
        /// Tests that GetMessage respects the visibility timeout
        /// </summary>
        [TestMethod]
        public void GetMessage_ShouldRespectVisibilityTimeout()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            // call get message twice to make sure both returns
            QueueMessage msg1 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            QueueMessage msg2 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;

            Assert.IsNotNull(msg2, "GetMessage did not return anythingt");
            Assert.AreEqual(msg1.MessageId, msg2.MessageId, "GetMessage did not resturn the same message");

            this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(5), cancellationToken: CancellationToken.None).Wait();
            Assert.IsNull(
                this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(5), cancellationToken: CancellationToken.None).Result,
                "GetMessage should not return anything for another five seconds");

            Thread.Sleep(TimeSpan.FromSeconds(5));
            
            Assert.IsNotNull(
                this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(5), cancellationToken: CancellationToken.None).Result,
                "GetMessage is expected to return the message");
        }

        /// <summary>
        /// Tests that GetMessage respects the visibility timeout
        /// </summary>
        [TestMethod]
        public void GetMessage_ShouldNotReturnExpiredMessages()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromSeconds(5),
                cancellationToken: CancellationToken.None).Wait();

            Assert.IsNotNull(
                this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result,
                "GetMessage is expected to return a message");

            // wait for the message to expire
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsNull(
                this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result,
                "GetMessage should not return expired messages");
        }

        /// <summary>
        /// Tests delete message
        /// </summary>
        [TestMethod]
        public void DeleteMessage_ShouldNotReturnAnything()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            // call get message twice to make sure both returns
            QueueMessage msg1 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            QueueMessage msg2 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;

            Assert.IsNotNull(msg2, "GetMessage did not return anything despite having zero visibility timeout");
            Assert.AreEqual(msg1.MessageId, msg2.MessageId, "GetMessage did not resturn the same message");

            this.Store.DeleteMessage(msg2, CancellationToken.None).Wait();
            Assert.IsNull(
                this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result,
                "Nothing is expected to return after deletion");
        }

        /// <summary>
        /// Tests deleting a message that does not exist
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DeleteMessageException))]
        public void DeleteMessage_ShouldThrowExceptionWhenMessgeDoesNotExist()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            // call get message twice to make sure both returns
            QueueMessage msg = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;

            this.Store.DeleteMessage(msg, CancellationToken.None).Wait();

            try
            {
                this.Store.DeleteMessage(msg, CancellationToken.None).Wait();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Tests deleting a message that got checked out by others
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DeleteMessageException))]
        public void DeleteMessage_ShouldThrowExceptionWhenCheckedOutByOthers()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            // checkout the message twice
            QueueMessage msg1 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            QueueMessage msg2 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            
            try
            {
                // this should throw an exception, since the same message is checked out by someone else (msg2)
                this.Store.DeleteMessage(msg1, CancellationToken.None).Wait();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }
        
        /// <summary>
        /// Initializes the queue store used for testing
        /// </summary>
        protected virtual void InitStore()
        {
            this.Store = new InMemoryQueueStore();
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
