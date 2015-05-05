namespace SabbathText.V1
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using QueueStorage;
    using SabbathText.Compensation.V1;

    /// <summary>
    /// A worker that processes the pending checkpoints
    /// </summary>
    public class CheckpointWorker : Worker
    {
        /// <summary>
        /// Finds a checkpoint and processes it
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A time span to delay until the next iteration.</returns>
        public override async Task<TimeSpan> RunIteration(CancellationToken cancellationToken)
        {
            GoodieBag bag = GoodieBag.Create();

            QueueMessage queueMessage = await bag.CompensationClient.GetCheckpointMessage(cancellationToken);

            if (queueMessage == null)
            {
                return bag.Settings.CheckpointWorkerIdleDelay;
            }
                        
            Checkpoint checkpoint = await bag.CompensationClient.GetCheckpoint(queueMessage, cancellationToken);

            if (checkpoint != null)
            {
                Trace.TraceInformation("Checkpoint {0}/{1} found".InvariantFormat(checkpoint.PartitionKey, checkpoint.RowKey));
                
                OperationCheckpointHandler handler = new OperationCheckpointHandler();
                await handler.Finish(checkpoint, cancellationToken);

                Trace.TraceInformation("Checkpoint {0}/{1} completed".InvariantFormat(checkpoint.PartitionKey, checkpoint.RowKey));
            }

            await bag.CompensationClient.DeleteCheckpointMessge(queueMessage, cancellationToken);

            return TimeSpan.Zero;
        }
    }
}
