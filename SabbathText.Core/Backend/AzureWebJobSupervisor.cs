using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public class AzureWebJobSupervisor : ConsoleSupervisor
    {
        private string shutdownFile = null;

        public AzureWebJobSupervisor()
        {
            this.shutdownFile = Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");
            this.StartWatchingShutdownFile();
        }

        private void StartWatchingShutdownFile()
        {
            if (string.IsNullOrWhiteSpace(this.shutdownFile))
            {
                return;
            }

            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(this.shutdownFile));
            watcher.Created += this.ShutdownDirectoryChanged;
            watcher.Changed += this.ShutdownDirectoryChanged;
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }

        private void ShutdownDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(this.shutdownFile))
            {
                this.RequestStop();
            }
        }
    }
}
