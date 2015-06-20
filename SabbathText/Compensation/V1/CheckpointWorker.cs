namespace SabbathText.Compensation.V1
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using QueueStorage;

    /// <summary>
    /// A worker that processes the pending checkpoints
    /// </summary>
    public class CheckpointWorker : Worker
    {
        private CompensationClient compensationClient;
        private TimeSpan idleDelay;
        private CheckpointHandler handler;

        /// <summary>
        /// Creates a new instance of the worker
        /// </summary>
        /// <param name="compensationClient">The compensation client.</param>
        /// <param name="idleDelay">The delay between run iterations if no work is found.</param>
        /// <param name="handler">The checkpoint handler.</param>
        public CheckpointWorker(CompensationClient compensationClient, TimeSpan idleDelay, CheckpointHandler handler)
        {
            this.compensationClient = compensationClient;
            this.idleDelay = idleDelay;
            this.handler = handler;
        }

        /// <summary>
        /// Finds a checkpoint and processes it
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A time span to delay until the next iteration.</returns>
        public override async Task<TimeSpan> RunIteration(CancellationToken cancellationToken)
        {
            QueueMessage queueMessage = await this.compensationClient.GetMessage(cancellationToken);

            if (queueMessage == null)
            {
                return this.idleDelay;
            }

            Checkpoint checkpoint = await this.compensationClient.GetCheckpoint(queueMessage, cancellationToken);

            if (checkpoint != null)
            {
                Trace.TraceInformation(string.Format("Checkpoint {0}/{1} found", checkpoint.PartitionKey, checkpoint.RowKey));
                bool completed = await this.handler.Finish(checkpoint, cancellationToken);
                Trace.TraceInformation(string.Format("Checkpoint {0}/{1} completed", checkpoint.PartitionKey, checkpoint.RowKey));

                if (completed)
                {
                    Trace.TraceInformation("Deleting queue message");
                    await this.compensationClient.DeleteMessge(queueMessage, cancellationToken);
                }
            }
            else
            {
                Trace.TraceInformation(string.Format("Checkpoint not found, deleting queue message"));
                await this.compensationClient.DeleteMessge(queueMessage, cancellationToken);
            }

            return TimeSpan.Zero;
        }
    }
}
