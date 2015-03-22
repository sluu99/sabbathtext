using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;

namespace QueueStorage
{
    public class AzureQueue : IQueue
    {
        static readonly TimeSpan TimeToLive = TimeSpan.FromDays(7);

        private CloudStorageAccount account = null;
        private CloudQueueClient client = null;

        public AzureQueue()
        {
            string connectionString = Environment.GetEnvironmentVariable("ST_QUEUE_CONN_STR");
            this.account = CloudStorageAccount.DevelopmentStorageAccount;
            
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                this.account = CloudStorageAccount.Parse(connectionString);
            }

            this.client = this.account.CreateCloudQueueClient();
            this.GetMessageVisibilityTimeout = TimeSpan.FromMinutes(5);
        }

        public TimeSpan GetMessageVisibilityTimeout { get; set; }
        
        public Task CreateIfNotExists(string queueName)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);            
            return queue.CreateIfNotExistsAsync();
        }

        public Task AddMessage(string queueName, CloudQueueMessage message)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);            
            return queue.AddMessageAsync(message);
        }

        public Task AddMessage(string queueName, CloudQueueMessage message, TimeSpan visibilityDelay)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);
            return queue.AddMessageAsync(message, AzureQueue.TimeToLive, visibilityDelay, null, null);
        }

        public Task<CloudQueueMessage> GetMessage(string queueName)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);
            return queue.GetMessageAsync(this.GetMessageVisibilityTimeout, null, null);
        }

        public Task DeleteMessage(string queueName, CloudQueueMessage message)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);
            return queue.DeleteMessageAsync(message);
        }
    }
}
