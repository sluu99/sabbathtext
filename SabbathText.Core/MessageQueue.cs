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
        public const string EventMessageQueue = "eventmsgs";

        private string queueName;

        public MessageQueue(string queueName)
        {
            this.Queue = new AzureQueue();
            this.queueName = queueName;
        }

        public IQueue Queue { get; set; }

        public Task DeleteMessage(CloudQueueMessage queueMessage)
        {
            return this.Queue.DeleteMessage(this.queueName, queueMessage);
        }

        public async Task<Tuple<CloudQueueMessage, Message>> GetMessage()
        {
            CloudQueueMessage queueMessage = await this.Queue.GetMessage(this.queueName);

            if (queueMessage == null)
            {
                return null;
            }

            return new Tuple<CloudQueueMessage,Message>(queueMessage, JsonConvert.DeserializeObject<Message>(queueMessage.AsString));
        }

        public Task AddMessage(Message message)
        {
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message));
            return this.Queue.AddMessage(this.queueName, queueMessage);
        }
    }
}
