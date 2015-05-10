namespace QueueStorage.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This class tests the QueueStore class
    /// </summary>
    [TestClass]
    public class QueueStoreTests
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private const string ConnectionString = "UseDevelopmentStorage=true";

        /// <summary>
        /// The queue name
        /// </summary>
        private string queueName = "test";

        /// <summary>
        /// Gets or sets the queue store used for testing
        /// </summary>
        protected QueueStore Store { get; set; }

        /// <summary>
        /// This method will be called before every test run
        /// </summary>
        [TestInitialize]
        public virtual void Init()
        {
            this.InitStore();
        }

        /// <summary>
        /// this method will be called after test runs
        /// </summary>
        [TestCleanup]
        public virtual void CleanUp()
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
            Clock.Delay(TimeSpan.FromSeconds(1)).Wait();
            QueueMessage msg2 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;

            Assert.IsNotNull(msg2, "GetMessage did not return anythingt");
            Assert.AreEqual(msg1.MessageId, msg2.MessageId, "GetMessage did not resturn the same message");

            this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(5), cancellationToken: CancellationToken.None).Wait();
            Assert.IsNull(
                this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(5), cancellationToken: CancellationToken.None).Result,
                "GetMessage should not return anything for another five seconds");

            Clock.Delay(TimeSpan.FromSeconds(5)).Wait();

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
            Clock.Delay(TimeSpan.FromSeconds(5)).Wait();

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
            Clock.Delay(TimeSpan.FromSeconds(1)).Wait();
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
        [ExpectedException(typeof(MessageNotFoundException))]
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
        [ExpectedException(typeof(MessageNotFoundException))]
        public void DeleteMessage_ShouldThrowExceptionWhenCheckedOutByOthers()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            // checkout the message twice
            QueueMessage msg1 = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            Clock.Delay(TimeSpan.FromSeconds(2)).Wait();
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
        /// Test that the queue does not return any message with timeout extended
        /// </summary>
        [TestMethod]
        public void ExtendTimeout_ShouldNotReturnAnyMessage()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            QueueMessage msg = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            this.Store.ExtendTimeout(msg, TimeSpan.FromSeconds(3), CancellationToken.None).Wait();

            Clock.Delay(TimeSpan.FromSeconds(1.5)).Wait();
            Assert.IsNull(
                this.Store.GetMessage(TimeSpan.FromSeconds(1), CancellationToken.None).Result,
                "The queue should not return any message");

            Clock.Delay(TimeSpan.FromSeconds(2)).Wait(); // wait until the lock extension expires
            Assert.IsNotNull(
                this.Store.GetMessage(TimeSpan.FromSeconds(1), CancellationToken.None).Result,
                "The queue should returns the message after the lock extention expires");
        }

        /// <summary>
        /// Test that the queue throws an exception when trying to extends the time out of a non-existing message
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MessageNotFoundException))]
        public void ExtendTimeout_ShouldThrownWhenMessageDoesNotExist()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            QueueMessage msg = this.Store.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None).Result;
            this.Store.DeleteMessage(msg, CancellationToken.None).Wait();

            try
            {
                this.Store.ExtendTimeout(msg, TimeSpan.FromSeconds(3), CancellationToken.None).Wait();
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
        /// Test that the queue throws an exception when trying to extends the time out of a message
        /// checked out by a different process
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MessageNotFoundException))]
        public void ExtendTimeout_ShouldThrownWhenMessageCheckedOutByOthers()
        {
            this.Store.AddMessage(
                "hello",
                visibilityDelay: TimeSpan.Zero,
                messageLifeSpan: TimeSpan.FromDays(7),
                cancellationToken: CancellationToken.None).Wait();

            QueueMessage msg1 = this.Store.GetMessage(TimeSpan.FromSeconds(1), CancellationToken.None).Result;
            Clock.Delay(TimeSpan.FromSeconds(2)).Wait();
            QueueMessage msg2 = this.Store.GetMessage(TimeSpan.FromSeconds(1), CancellationToken.None).Result;

            Assert.IsNotNull(msg2, "Could not check out message");

            try
            {
                this.Store.ExtendTimeout(msg1, TimeSpan.FromSeconds(3), CancellationToken.None).Wait();
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
            this.queueName = "test" + Guid.NewGuid().ToString().Substring(0, 8);
            this.Store = new QueueStore();
            this.Store.InitAzureQueue(ConnectionString, this.queueName);
        }

        /// <summary>
        /// Clean up the store after testing
        /// </summary>
        protected virtual void CleanUpStore()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
            account.CreateCloudQueueClient().GetQueueReference(this.queueName).DeleteIfExists();
        }
    }
}
