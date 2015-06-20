namespace SabbathText.Compensation.V1
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using QueueStorage;

    /// <summary>
    /// The compensation client
    /// </summary>
    public class CompensationClient
    {
        private readonly TimeSpan checkpointLifespan = TimeSpan.FromDays(7);

        private TimeSpan checkpointTimeout;
        private KeyValueStore<Checkpoint> checkpointStore;
        private QueueStore checkpointQueue;

        /// <summary>
        /// Creates a new instance of the compensation client
        /// </summary>
        /// <param name="checkpointStore">The checkpoint store</param>
        /// <param name="checkpointQueue">The checkpoint queue</param>
        /// <param name="checkpointTimeout">The checkpoint timeout</param>
        public CompensationClient(KeyValueStore<Checkpoint> checkpointStore, QueueStore checkpointQueue, TimeSpan checkpointTimeout)
        {
            this.checkpointTimeout = checkpointTimeout;
            this.checkpointStore = checkpointStore;
            this.checkpointQueue = checkpointQueue;
        }

        /// <summary>
        /// Inserts a checkpoint
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The checkpoint itself</returns>
        public Task InsertCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            return this.checkpointStore.Insert(checkpoint, cancellationToken);
        }

        /// <summary>
        /// Gets an existing checkpoint.
        /// </summary>
        /// <param name="partitionKey">The checkpoint partition key.</param>
        /// <param name="rowKey">The checkpoint row key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The checkpoint.</returns>
        public Task<Checkpoint> GetCheckpoint(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            return this.checkpointStore.Get(partitionKey, rowKey, cancellationToken);
        }

        /// <summary>
        /// Puts a checkpoint into the work queue
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <param name="visibilityDelay">The timeout before this checkpoint could be picked up by the compensation agent</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The operation task</returns>
        public Task QueueCheckpoint(Checkpoint checkpoint, TimeSpan visibilityDelay, CancellationToken cancellationToken)
        {
            if (visibilityDelay < TimeSpan.Zero)
            {
                throw new ArgumentException("Visibility delay cannot be less than zero", "visibilityDelay");
            }

            EntityReference checkpointRef = new EntityReference
            {
                PartitionKey = checkpoint.PartitionKey,
                RowKey = checkpoint.RowKey,
            };

            return this.checkpointQueue.AddMessage(
                JsonConvert.SerializeObject(checkpointRef),
                visibilityDelay,
                this.checkpointLifespan,
                cancellationToken);
        }

        /// <summary>
        /// Updates a checkpoint
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The update task</returns>
        public Task UpdateCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            return this.checkpointStore.Update(checkpoint, cancellationToken);
        }

        /// <summary>
        /// Gets the next checkpoint message available
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A queue message</returns>
        public Task<QueueMessage> GetMessage(CancellationToken cancellationToken)
        {
            return this.checkpointQueue.GetMessage(this.checkpointTimeout, cancellationToken);
        }

        /// <summary>
        /// Gets a checkpoint from a checkpoint reference.
        /// </summary>
        /// <param name="checkpointRef">The checkpoint reference.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The checkpoint, or null if it does not exist.</returns>
        private Task<Checkpoint> GetCheckpoint(EntityReference checkpointRef, CancellationToken cancellationToken)
        {
            return this.checkpointStore.Get(checkpointRef.PartitionKey, checkpointRef.RowKey, cancellationToken);
        }

        /// <summary>
        /// Gets the checkpoint referenced by the queue message
        /// </summary>
        /// <param name="message">The queue message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A checkpoint</returns>
        public async Task<Checkpoint> GetCheckpoint(QueueMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentException("message");
            }

            EntityReference checkpointRef = JsonConvert.DeserializeObject<EntityReference>(message.Body);
            Checkpoint checkpoint = await this.checkpointStore.Get(checkpointRef.PartitionKey, checkpointRef.RowKey, cancellationToken);

            if (checkpoint != null)
            {
                checkpoint.QueueMessage = message;
            }

            return checkpoint;
        }

        /// <summary>
        /// Deletes a checkpoint message
        /// </summary>
        /// <param name="message">The checkpoint message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The delete task</returns>
        public Task DeleteMessge(QueueMessage message, CancellationToken cancellationToken)
        {
            return this.checkpointQueue.DeleteMessage(message, cancellationToken);
        }

        /// <summary>
        /// Extends a message's visibility timeout
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="timeout">The timeout</param>
        /// <param name="cancellationToken">the cancellation token.</param>
        /// <returns>A TPL task</returns>
        public Task ExtendMessageTimeout(QueueMessage message, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return this.checkpointQueue.ExtendTimeout(message, timeout, cancellationToken);
        }
    }
}
