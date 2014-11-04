using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;

namespace QueueStorage
{
    public class AzureQueue : IQueue
    {
        private string connectionString = null;
        private CloudStorageAccount account = null;
        private CloudQueueClient client = null;

        public AzureQueue()
        {
            this.connectionString = Environment.GetEnvironmentVariable("ST_QUEUE_CONN_STR");
            this.account = CloudStorageAccount.DevelopmentStorageAccount;
            
            if (!string.IsNullOrWhiteSpace(this.connectionString))
            {
                this.account = CloudStorageAccount.Parse(this.connectionString);
            }

            this.client = this.account.CreateCloudQueueClient();
        }
        
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

        public Task<CloudQueueMessage> GetMessage(string queueName)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);
            return queue.GetMessageAsync();
        }

        public Task DeleteMessage(string queueName, CloudQueueMessage message)
        {
            CloudQueue queue = this.client.GetQueueReference(queueName);
            return queue.DeleteMessageAsync(message);
        }
    }
}
