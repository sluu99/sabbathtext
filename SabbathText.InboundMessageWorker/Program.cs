using SabbathText.Core.Backend;
using SabbathText.Core.Entities;
using System.Threading.Tasks;

namespace SabbathText.InboundMessageWorker
{
    class Program
    {
        private static Supervisor supervisor = null;

        static void Main(string[] args)
        {                        
            Program.supervisor = new AzureWebJobSupervisor();
            Program.supervisor.Start(Program.ProcessMessage).Wait();
        }

        static Task<bool> ProcessMessage(Message message)
        {
            return Task.FromResult(false);
        }
    }
}
