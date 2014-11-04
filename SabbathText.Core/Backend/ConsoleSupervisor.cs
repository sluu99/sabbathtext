using System;
using System.Diagnostics;

namespace SabbathText.Core.Backend
{
    public class ConsoleSupervisor : Supervisor
    {
        public ConsoleSupervisor()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Console.CancelKeyPress += this.CancelKeyPress;
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            this.RequestStop();
            Trace.TraceInformation("Stop requested. Waiting for the process to finish...");
        }
    }
}
