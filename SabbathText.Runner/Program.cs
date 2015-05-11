﻿namespace SabbathText.CompensationAgent
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SabbathText.Compensation.V1;
    using SabbathText.V1;

    /// <summary>
    /// The main program
    /// </summary>
    public class Program
    {
        private const string ShutdownFileEnvironmentKey = "WEBJOBS_SHUTDOWN_FILE";

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
            GoodieBag.Initialize(EnvironmentSettings.Create());
            GoodieBag bag = GoodieBag.Create();

            Console.CancelKeyPress += this.CancelKeyPress;

            TraceListener traceListener = new ConsoleTraceListener();

            try
            {
                if (bag.Settings.WorkersUseConsoleTrace)
                {
                    Trace.Listeners.Add(traceListener);
                }

                this.StartWatchingShutdownFile();

                while (this.cancellationToken.IsCancellationRequested == false)
                {
                    Trace.TraceInformation("Hello from runner" + Clock.UtcNow);
                    Thread.Sleep(10000);
                }
            }
            finally
            {
                if (bag.Settings.WorkersUseConsoleTrace)
                {
                    Trace.Listeners.Remove(traceListener);
                }

                Console.CancelKeyPress -= this.CancelKeyPress;
            }
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            this.cancellationToken.Cancel();
            e.Cancel = true;
            Trace.TraceInformation("Stop requested. Waiting for the process to finish...");
        }

        private void StartWatchingShutdownFile()
        {
            string shutdownFile = Environment.GetEnvironmentVariable(ShutdownFileEnvironmentKey);
            if (string.IsNullOrWhiteSpace(shutdownFile))
            {
                Trace.TraceWarning("No shutdown file specified");
                return;
            }

            Trace.TraceInformation("Shut down file: {0}", shutdownFile);

            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(shutdownFile));
            watcher.Created += this.ShutdownDirectoryChanged;
            watcher.Changed += this.ShutdownDirectoryChanged;
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }

        private void ShutdownDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            string shutdownFile = Environment.GetEnvironmentVariable(ShutdownFileEnvironmentKey);
            Trace.TraceInformation("ShutdownDirectoryChanged");
            Trace.TraceInformation(e.FullPath);

            if (e.FullPath.IndexOf(Path.GetFileName(shutdownFile), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                this.cancellationToken.Cancel();
            }
        }
    }
}
