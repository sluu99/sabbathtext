using QueueStorage;
using System;

namespace SabbathText.Core
{
    public static class Common
    {
        private static void SetupEnvironmentVariables()
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("ST_TWILIO_INBOUND_KEY_PRIMARY") == null)
            {
                Environment.SetEnvironmentVariable("ST_TWILIO_INBOUND_KEY_PRIMARY", "key");
            }
#endif
        }

        private static void SetupQueues()
        {
            string[] queueNames = { MessageQueue.InboundMessageQueue, MessageQueue.OutboundMessageQueue };
            IQueue queue = new AzureQueue();

            foreach (string q in queueNames)
            {
                queue.CreateIfNotExists(q).Wait();
            }
        }

        public static void Setup()
        {
            Common.SetupEnvironmentVariables();
            Common.SetupQueues();
        }
    }
}
