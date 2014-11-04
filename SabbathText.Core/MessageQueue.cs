using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using QueueStorage;
using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class MessageQueue
    {
        public const string InboundMessageQueue = "inboundmsgs";
        public const string OutboundMessageQueue = "outboundmsgs";

        public MessageQueue()
        {
            Queue = new AzureQueue();
        }

        public IQueue Queue { get; set; }

        public Task QueueInboundMessage(Message message)
        {
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message));
            return Queue.AddMessage(MessageQueue.InboundMessageQueue, queueMessage);
        }

        public async Task<Tuple<CloudQueueMessage, Message>> GetInboundMessage()
        {
            CloudQueueMessage queueMessage = await Queue.GetMessage(MessageQueue.InboundMessageQueue);

            if (queueMessage == null)
            {
                return null;
            }

            return new Tuple<CloudQueueMessage,Message>(queueMessage, JsonConvert.DeserializeObject<Message>(queueMessage.AsString));
        }

        public Task DeleteInboundMessage(CloudQueueMessage queueMessage)
        {
            return Queue.DeleteMessage(MessageQueue.InboundMessageQueue, queueMessage);
        }
    }
}
