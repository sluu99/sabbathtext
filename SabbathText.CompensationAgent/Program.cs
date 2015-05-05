namespace SabbathText.CompensationAgent
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SabbathText.V1;

    /// <summary>
    /// The main program
    /// </summary>
    public class Program
    {
        private CancellationTokenSource cancellationToken;

        /// <summary>
        /// The application entry
        /// </summary>
        /// <param name="args">The application argument</param>
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        /// <summary>
        /// Run the compensation agent
        /// </summary>
        public void Run()
        {
            this.cancellationToken = new CancellationTokenSource();
            TraceListener traceListener = new ConsoleTraceListener();
            Trace.Listeners.Add(traceListener);
            Console.CancelKeyPress += this.CancelKeyPress;

            try
            {
                GoodieBag.Initialize(EnvironmentSettings.Create());
                GoodieBag bag = GoodieBag.Create();

                CheckpointWorker worker = new CheckpointWorker();
                worker.Run(bag.Settings.CheckpointWorkerIdleDelay, this.cancellationToken.Token).Wait();
            }
            finally
            {
                Trace.Listeners.Remove(traceListener);
                Console.CancelKeyPress -= this.CancelKeyPress;
            }
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            this.cancellationToken.Cancel();
            e.Cancel = true;
            Trace.TraceInformation("Stop requested. Waiting for the process to finish...");
        }
    }
}
