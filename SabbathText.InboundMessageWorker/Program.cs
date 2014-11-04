using SabbathText.Core.Backend;
using SabbathText.Core.Backend.InboundProcessors;

namespace SabbathText.InboundMessageWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            SabbathText.Core.Common.Setup();

            InboundMessageRouter router = new InboundMessageRouter();
            Program.AddProcessors(router);

            Supervisor supervisor = new AzureWebJobSupervisor();
            supervisor.Start(router.Route).Wait();
        }

        static void AddProcessors(InboundMessageRouter router)
        {
            router
                .AddProcessor<HelloProcessor>("hello")
                .AddProcessor<HelpProcessor>("help")
                .AddProcessor<HelpProcessor>("who")
                .AddProcessor<HelpProcessor>("?")
            ;
        }
    }
}
