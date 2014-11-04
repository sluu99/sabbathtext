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
            this.Queue = new AzureQueue();
        }

        public IQueue Queue { get; set; }

        public Task QueueInboundMessage(Message message)
        {
            return this.AddMessage(MessageQueue.InboundMessageQueue, message);
        }

        public async Task<Tuple<CloudQueueMessage, Message>> GetInboundMessage()
        {
            CloudQueueMessage queueMessage = await this.Queue.GetMessage(MessageQueue.InboundMessageQueue);

            if (queueMessage == null)
            {
                return null;
            }

            return new Tuple<CloudQueueMessage,Message>(queueMessage, JsonConvert.DeserializeObject<Message>(queueMessage.AsString));
        }

        public Task DeleteInboundMessage(CloudQueueMessage queueMessage)
        {
            return this.Queue.DeleteMessage(MessageQueue.InboundMessageQueue, queueMessage);
        }

        public Task QueueOutboundMessage(Message message)
        {
            return this.AddMessage(MessageQueue.OutboundMessageQueue, message);
        }

        private Task AddMessage(string queueName, Message message)
        {
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message));
            return this.Queue.AddMessage(queueName, queueMessage);
        }
    }
}
