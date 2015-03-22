using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;

namespace QueueStorage
{
    public interface IQueue
    {
        Task CreateIfNotExists(string queueName);
        Task AddMessage(string queueName, CloudQueueMessage message);
        Task AddMessage(string queueName, CloudQueueMessage message, TimeSpan visibilityDelay);
        Task<CloudQueueMessage> GetMessage(string queueName);
        Task DeleteMessage(string queueName, CloudQueueMessage message);
    }
}
