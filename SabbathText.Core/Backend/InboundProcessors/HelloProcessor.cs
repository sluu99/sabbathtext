using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class HelloProcessor : IProcessor
    {
        public HelloProcessor()
        {
            this.MessageQueue = new MessageQueue();
        }

        public MessageQueue MessageQueue { get; set; }

        public async Task<bool> ProcessMessage(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Sender))
            {
                throw new ApplicationException("Message sender cannot be null or white space");
            }

            Message response = MessageFactory.CreateGreetingsMessage(message.Sender);
            await this.MessageQueue.QueueOutboundMessage(response);

            Trace.TraceInformation("Greeting message queued for {0}", response.Recipient.Mask(2));

            return true;
        }
    }
}
