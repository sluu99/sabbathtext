namespace QueueStorage
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// A queue implementation backed by Azure queue
    /// </summary>
    public class QueueStore : InMemoryQueueStore
    {
        private CloudQueue cloudQueue;

        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="queueName">The queue name</param>
        public void InitAzureQueue(string connectionString, string queueName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required", "connectionString");
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("Table queue name is required", "queueName");
            }

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            this.cloudQueue = queueClient.GetQueueReference(queueName);
            this.cloudQueue.CreateIfNotExists();
        }

        /// <summary>
        /// Adds a new message to the queue
        /// </summary>
        /// <param name="body">The message body</param>
        /// <param name="visibilityDelay">The amount of time before the message is visible</param>
        /// <param name="messageLifeSpan">The amount of time that the message lives</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The operation task</returns>
        public override Task AddMessage(string body, TimeSpan visibilityDelay, TimeSpan messageLifeSpan, CancellationToken cancellationToken)
        {
            CloudQueueMessage message = new CloudQueueMessage(body);
            return this.cloudQueue.AddMessageAsync(
                message,
                timeToLive: messageLifeSpan,
                initialVisibilityDelay: visibilityDelay,
                options: null,
                operationContext: null,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the next message from the queue
        /// </summary>
        /// <param name="visibilityTimeout">The amount of time the returned message will stay hidden until it becomes visible again</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A queue message, or null of there's none available</returns>
        public override async Task<QueueMessage> GetMessage(TimeSpan visibilityTimeout, CancellationToken cancellationToken)
        {
            if (visibilityTimeout < TimeSpan.FromSeconds(1))
            {
                throw new ArgumentException("Visibility timeout must be at least 1 second", "visibilityTimeout");
            }

            CloudQueueMessage cloudMessage =
                    await this.cloudQueue.GetMessageAsync(visibilityTimeout, null /* options */, null /* operation context */, cancellationToken);

            if (cloudMessage == null)
            {
                return null;
            }

            QueueMessage msg = new QueueMessage
            {
                MessageId = cloudMessage.Id,
                Body = cloudMessage.AsString,
                DequeueCount = cloudMessage.DequeueCount,
                ImplementationData = cloudMessage.PopReceipt,
            };

            if (cloudMessage.InsertionTime != null)
            {
                msg.InsertionTime = cloudMessage.InsertionTime.Value.UtcDateTime;
            }

            if (cloudMessage.ExpirationTime != null)
            {
                msg.ExpirationTime = cloudMessage.ExpirationTime.Value.UtcDateTime;
            }

            if (cloudMessage.NextVisibleTime != null)
            {
                msg.ExpirationTime = cloudMessage.NextVisibleTime.Value.UtcDateTime;
            }

            return msg;
        }

        /// <summary>
        /// Deletes a message from the queue
        /// </summary>
        /// <param name="message">The queue message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The deletion task</returns>
        public override async Task DeleteMessage(QueueMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (string.IsNullOrWhiteSpace(message.MessageId))
            {
                throw new ArgumentException("The message ID is required");
            }

            try
            {
                await this.cloudQueue.DeleteMessageAsync(
                    message.MessageId,
                    popReceipt: message.ImplementationData,
                    cancellationToken: cancellationToken);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404 &&
                    "MessageNotFound".Equals(ex.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture))
                {
                    throw new DeleteMessageException();
                }

                throw;
            }
        }
    }
}
