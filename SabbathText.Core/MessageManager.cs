using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using QueueStorage;
using SabbathText.Core.Entities;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class MessageManager
    {
        public const string InboundMessageQueue = "inboundmsgs";

        public MessageManager()
        {
            Queue = new AzureQueue();
        }

        public IQueue Queue { get; set; }

        public Task QueueInboundMessage(Message message)
        {
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message));
            return Queue.AddMessage(MessageManager.InboundMessageQueue, queueMessage);
        }
    }
}
