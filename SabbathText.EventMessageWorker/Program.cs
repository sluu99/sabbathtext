using SabbathText.Core;
using SabbathText.Core.Backend;

namespace SabbathText.EventMessageWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            SabbathText.Core.Common.Setup();
            SabbathText.Core.Common.SetupStorage();

            MessageRouter router = new MessageRouter();
            Program.AddProcessors(router);

            Supervisor supervisor = new AzureWebJobSupervisor(MessageQueue.EventMessageQueue);
            supervisor.Start(router.Route).Wait();
        }

        static void AddProcessors(MessageRouter router)
        {
            
        }
    }
}
