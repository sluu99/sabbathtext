using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class HelpProcessor : IProcessor
    {
        public HelpProcessor()
        {
            this.OutboundMessageQueue = new MessageQueue(MessageQueue.OutboundMessageQueue);
        }

        public MessageQueue OutboundMessageQueue { get; set; }

        public Task<TemplatedMessage> ProcessMessage(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Sender))
            {
                throw new ApplicationException("Message sender cannot be null or white space");
            }

            return Task.FromResult(new TemplatedMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Recipient = message.Sender,
                Template = MessageTemplate.Help,
                CreateOn = Clock.UtcNow,
                Body = "Go to http://www.SabbathText.com for more information, or text \"stop\" to opt out from receiving more messages.",
            });
        }
    }
}
