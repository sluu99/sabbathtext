using SabbathText.Core;
using SabbathText.Core.Backend;
using SabbathText.Core.Backend.EventProcessors;
using SabbathText.Core.Entities;

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
            router
                .AddProcessor<ZipCodeUpdatedProcessor>(EventType.ZipCodeUpdated.ToString())
                .AddProcessor<SabbathProcessor>(EventType.Sabbath.ToString())
                .AddProcessor<AccountCycleProcessor>(EventType.AccountCycle.ToString())
                .AddProcessor<GreetingsRequestedProcessor>(EventType.GreetingsRequested.ToString())
            ;
        }
    }
}
