namespace SabbathText.Compensation.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using QueueStorage;

    /// <summary>
    /// The compensation processor
    /// </summary>
    public class CompensationProcessor
    {
        /// <summary>
        /// The amount of time to wait to retry the queue again when no checkpoint returns
        /// </summary>
        private static readonly TimeSpan DelayWhenNoCheckpoint = TimeSpan.FromSeconds(3);

        private CompensationClient compensationClient;
        private CheckpointHandler handler;
        private TimeSpan operationTimeout;
        private int poisonThreshold;

        /// <summary>
        /// Creates a new instance of the compensation processor
        /// </summary>
        /// <param name="compensationClient">The compensation client</param>
        /// <param name="handler">The checkpoint handler</param>
        /// <param name="operationTimeout">Operation timeout</param>
        /// <param name="poisonCheckpointThreshold">The number de-queue count for a message be considered poison</param>
        public CompensationProcessor(
            CompensationClient compensationClient,
            CheckpointHandler handler,
            TimeSpan operationTimeout,
            int poisonCheckpointThreshold)
        {
            this.compensationClient = compensationClient;
            this.handler = handler;
            this.operationTimeout = operationTimeout;
            this.poisonThreshold = poisonCheckpointThreshold;
        }
        
        /// <summary>
        /// Process the compensation queue until it sees a particular tracking ID
        /// </summary>
        /// <returns>The process task</returns>
        public async Task Process()
        {
            while (true)
            {
                Checkpoint checkpoint;

                try
                {
                    QueueMessage message = await this.compensationClient.GetCheckpointMessage(
                        new CancellationTokenSource(this.operationTimeout).Token);

                    if (message == null)
                    {
                        await Task.Delay(DelayWhenNoCheckpoint);
                        continue;
                    }

                    checkpoint = await this.compensationClient.GetCheckpoint(
                        message,
                        new CancellationTokenSource(this.operationTimeout).Token);

                    if (checkpoint == null)
                    {
                        continue;
                    }

                    await this.handler.Finish(
                        checkpoint,
                        new CancellationTokenSource(this.operationTimeout).Token);

                    if (checkpoint.Status == CheckpointStatus.Completed || checkpoint.Status == CheckpointStatus.Cancelled)
                    {
                        await this.compensationClient.DeleteCheckpointMessge(
                            message,
                            new CancellationTokenSource(this.operationTimeout).Token);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("{0} \r\n {1}", ex.Message, ex.StackTrace);
                }
            }
        }
    }
}
