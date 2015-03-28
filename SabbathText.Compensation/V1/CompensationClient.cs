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
        /// Inserts or get a checkpoint
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The checkpoint itself</returns>
        public async Task<Checkpoint> InsertOrGetCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            checkpoint = await this.checkpointStore.InsertOrGet(checkpoint, cancellationToken);

            CheckpointReference checkpointRef = new CheckpointReference
            {
                PartitionKey = checkpoint.PartitionKey,
                RowKey = checkpoint.RowKey,
            };

            await this.checkpointQueue.AddMessage(
                JsonConvert.SerializeObject(checkpointRef),
                this.checkpointTimeout,
                this.checkpointLifespan,
                cancellationToken);

            return checkpoint;
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
            CheckpointReference checkpointRef = new CheckpointReference
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
        public Task<QueueMessage> GetCheckpointMessage(CancellationToken cancellationToken)
        {
            return this.checkpointQueue.GetMessage(this.checkpointTimeout, cancellationToken);
        }

        /// <summary>
        /// Gets the checkpoint referenced by the queue message
        /// </summary>
        /// <param name="message">The queue message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A checkpoint</returns>
        public Task<Checkpoint> GetCheckpoint(QueueMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentException("message");
            }

            CheckpointReference checkpointRef = JsonConvert.DeserializeObject<CheckpointReference>(message.Body);
            return this.checkpointStore.Get(checkpointRef.PartitionKey, checkpointRef.RowKey);
        }

        /// <summary>
        /// Deletes a checkpoint message
        /// </summary>
        /// <param name="message">The checkpoint message</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The delete task</returns>
        public Task DeleteCheckpointMessge(QueueMessage message, CancellationToken cancellationToken)
        {
            return this.checkpointQueue.DeleteMessage(message, cancellationToken);
        }
    }
}
