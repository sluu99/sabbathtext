using SabbathText.Core;
using SabbathText.Core.Backend;
using SabbathText.Core.Entities;
using System.Threading.Tasks;

namespace SabbathText.OutboundMessageWorker
{
    class Program
    {
        static IMessageSender sender;

        static void Main(string[] args)
        {
            SabbathText.Core.Common.Setup();

            Program.sender = new TwilioMessageSender();

            Supervisor supervisor = new AzureWebJobSupervisor(MessageQueue.OutboundMessageQueue);
            supervisor.Start(Program.SendMessage).Wait();
        }

        async static Task<bool> SendMessage(Message message)
        {
            await sender.Send(message);
            return true;
        }
    }
}
