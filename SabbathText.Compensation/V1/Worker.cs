namespace SabbathText.Compensation.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A worker processor
    /// </summary>
    public abstract class Worker
    {
        /// <summary>
        /// Runs an iteration.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The amount of time to wait until the next iteration.</returns>
        public abstract Task<TimeSpan> RunIteration(CancellationToken cancellationToken);

        /// <summary>
        /// Runs the worker.
        /// </summary>
        /// <param name="defaultDelay">The default delay if an iteration errors out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task Run(TimeSpan defaultDelay, CancellationToken cancellationToken)
        {
            Trace.TraceInformation("Worker started");

            while (cancellationToken.IsCancellationRequested == false)
            {
                bool exceptionCaught = false;

                try
                {
                    TimeSpan delay = await this.RunIteration(cancellationToken);
                    await Clock.Delay(delay);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    Trace.TraceError(ex.StackTrace);
                    exceptionCaught = true;
                }

                if (exceptionCaught)
                {
                    await Clock.Delay(defaultDelay);
                }
            }

            Trace.TraceInformation("Worker stopped");
        }
    }
}
