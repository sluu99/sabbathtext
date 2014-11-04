using QueueStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public static class Common
    {
        private static void SetupEnvironmentVariables()
        {
#if DEBUG
            Environment.SetEnvironmentVariable("ST_TWILIO_INBOUND_KEY_PRIMARY", "key");
#endif
        }

        private static void SetupQueues()
        {
            string[] queueNames = { MessageManager.InboundMessageQueue };
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
