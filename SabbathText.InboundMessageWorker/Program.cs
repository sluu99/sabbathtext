using SabbathText.Core;
using SabbathText.Core.Backend;
using SabbathText.Core.Backend.InboundProcessors;

namespace SabbathText.InboundMessageWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            SabbathText.Core.Common.Setup();
            SabbathText.Core.Common.SetupStorage();

            MessageRouter router = MessageRouter.NewInboundRouter();            

            Supervisor supervisor = new AzureWebJobSupervisor(MessageQueue.InboundMessageQueue);
            supervisor.Start(router.Route).Wait();
        }
    }
}
