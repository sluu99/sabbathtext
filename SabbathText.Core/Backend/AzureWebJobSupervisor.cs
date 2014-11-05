using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public class AzureWebJobSupervisor : ConsoleSupervisor
    {
        private string shutdownFile = null;

        public AzureWebJobSupervisor(string queueName) : base(queueName)
        {
            this.shutdownFile = Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");            
            this.StartWatchingShutdownFile();
        }

        private void StartWatchingShutdownFile()
        {
            if (string.IsNullOrWhiteSpace(this.shutdownFile))
            {
                Trace.TraceWarning("No shutdown file specified");
                return;
            }

            Trace.TraceInformation("Shut down file: {0}", this.shutdownFile);

            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(this.shutdownFile));
            watcher.Created += this.ShutdownDirectoryChanged;
            watcher.Changed += this.ShutdownDirectoryChanged;
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            
        }

        private void ShutdownDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            Trace.TraceInformation("ShutdownDirectoryChanged");
            Trace.TraceInformation(e.FullPath);

            if (e.FullPath.IndexOf(Path.GetFileName(this.shutdownFile), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                this.RequestStop();
            }
        }
    }
}
