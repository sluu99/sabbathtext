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
        /// Updates a checkpoint
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The update task</returns>
        public Task UpdateCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            return this.checkpointStore.Update(checkpoint, cancellationToken);
        }
    }
}
