using SabbathText.Core;
using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.InboundMessageWorker
{
    class Program
    {
        private static Supervisor supervisor = null;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += CancelKeyPress;
            Trace.Listeners.Add(new ConsoleTraceListener());
                        
            Program.supervisor = new Supervisor();
            Program.supervisor.Start(Program.ProcessMessage).Wait();
        }

        static Task<bool> ProcessMessage(Message message)
        {
            return Task.FromResult(false);
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Program.supervisor.RequestStop();

            Trace.TraceInformation("Waiting for the process to finish...");
        }
    }
}
