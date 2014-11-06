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

            MessageRouter router = new MessageRouter();
            Program.AddProcessors(router);

            Supervisor supervisor = new AzureWebJobSupervisor(MessageQueue.InboundMessageQueue);
            supervisor.Start(router.Route).Wait();
        }

        static void AddProcessors(MessageRouter router)
        {
            router
                .AddProcessor<HelloProcessor>("hello")
                .AddProcessor<HelloProcessor>("hi")
                
                .AddProcessor<HelpProcessor>("help")
                .AddProcessor<HelpProcessor>("who")
                .AddProcessor<HelpProcessor>("?")

                .AddProcessor<SubscribeProcessor>("subscribe")

                .AddProcessor<ZipCodeProcessor>("zipcode")
                .AddProcessor<ZipCodeProcessor>("zip")
            ;
        }
    }
}
