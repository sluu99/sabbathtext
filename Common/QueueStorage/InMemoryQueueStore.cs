namespace QueueStorage
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// This class provides an in-memory implementation of the queue store
    /// </summary>
    public class InMemoryQueueStore
    {
        /// <summary>
        /// The internal storage
        /// </summary>
        private LinkedList<QueueMessage> queue;

        /// <summary>
        /// Initializes the in-memory queue
        /// </summary>
        public void InitMemory()
        {
            this.queue = new LinkedList<QueueMessage>();
        }

        /// <summary>
        /// Adds a new message to the queue
        /// </summary>
        /// <param name="body">The message body</param>
        /// <param name="visibilityDelay">The amount of time before the message is visible</param>
        /// <param name="messageLifeSpan">The amount of time that the message lives</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The operation task</returns>
        public virtual Task AddMessage(string body, TimeSpan visibilityDelay, TimeSpan messageLifeSpan, CancellationToken cancellationToken)
        {
            QueueMessage message = new QueueMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                DequeueCount = 0,
                ExpirationTime = DateTime.UtcNow + messageLifeSpan,
                InsertionTime = DateTime.UtcNow,
                NextVisibleTime = DateTime.UtcNow + visibilityDelay,
                ImplementationData = Guid.NewGuid().ToString(), // etag
            };

            lock (this.queue)
            {
                this.queue.AddLast(message);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Gets the next message from the queue
        /// </summary>
        /// <param name="visibilityTimeout">The amount of time the returned message will stay hidden until it becomes visible again</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A queue message, or null of there's none available</returns>
        public virtual Task<QueueMessage> GetMessage(TimeSpan visibilityTimeout, CancellationToken cancellationToken)
        {
            if (visibilityTimeout < TimeSpan.FromSeconds(1))
            {
                throw new ArgumentException("Visibility timeout must be at least 1 second", "visibilityTimeout");
            }

            lock (this.queue)
            {
                LinkedListNode<QueueMessage> node = this.queue.First;
                while (node != null)
                {
                    LinkedListNode<QueueMessage> nextNode = node.Next;

                    if (node.Value.ExpirationTime <= DateTime.UtcNow)
                    {
                        // remove messages that have expired
                        this.queue.Remove(node); // O(1)
                    }
                    else if (node.Value.NextVisibleTime <= DateTime.UtcNow)
                    {
                        // found a visible message
                        node.Value.DequeueCount++;
                        node.Value.NextVisibleTime += visibilityTimeout;
                        node.Value.ImplementationData = Guid.NewGuid().ToString(); // etag

                        // this is a quick and dirty way to clone an object
                        return Task.FromResult(
                            JsonConvert.DeserializeObject<QueueMessage>(
                                JsonConvert.SerializeObject(node.Value)));
                    }

                    node = nextNode;
                }
            }

            return Task.FromResult<QueueMessage>(null);
        }

        /// <summary>
        /// Deletes a message from the queue
        /// </summary>
        /// <param name="message">The queue message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The deletion task</returns>
        public virtual Task DeleteMessage(QueueMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (string.IsNullOrWhiteSpace(message.MessageId))
            {
                throw new ArgumentException("The message ID is required");
            }

            bool messageFound = false;

            lock (this.queue)
            {
                LinkedListNode<QueueMessage> node = this.queue.First;
                while (node != null)
                {
                    LinkedListNode<QueueMessage> nextNode = node.Next;

                    if (node.Value.ExpirationTime <= DateTime.UtcNow || 
                        (node.Value.MessageId == message.MessageId && node.Value.ImplementationData == message.ImplementationData /* etag */))
                    {
                        // remove messages that have expired
                        this.queue.Remove(node); // O(1)

                        if (node.Value.MessageId == message.MessageId && node.Value.ImplementationData == message.ImplementationData /* etag */)
                        {
                            messageFound = true;
                            break;
                        }
                    }

                    node = nextNode;
                }
            }

            if (!messageFound)
            {
                throw new DeleteMessageException();
            }

            return Task.FromResult<object>(null);
        }
    }
}
