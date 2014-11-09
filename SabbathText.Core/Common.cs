using QueueStorage;
using System;

namespace SabbathText.Core
{
    public static class Common
    {
        private static void SetupQueues()
        {
            string[] queueNames = 
            {
                MessageQueue.InboundMessageQueue, 
                MessageQueue.OutboundMessageQueue,
                MessageQueue.EventMessageQueue,
            };

            IQueue queue = new AzureQueue();

            foreach (string q in queueNames)
            {
                queue.CreateIfNotExists(q).Wait();
            }
        }

        private static void SetupTables()
        {
            string[] tableNames = 
            { 
                AzureDataProvider.AccountTable,
                AzureDataProvider.AccountByIdentityTable,
                AzureDataProvider.MessageTable,
                AzureDataProvider.LocationByZipTable,
                AzureDataProvider.PoisonMessageTable,
                AzureDataProvider.KeyValueTable,
            };

            AzureDataProvider dp = new AzureDataProvider();

            foreach (string t in tableNames)
            {
                dp.CreateTableIfNotExists(t).Wait();
            }
        }

        public static void SetupStorage()
        {
            Common.SetupQueues();
            Common.SetupTables();
        }

        public static void Setup()
        {            
        }
    }
}
