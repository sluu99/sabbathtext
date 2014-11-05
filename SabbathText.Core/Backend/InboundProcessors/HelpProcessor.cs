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

        public async Task<bool> ProcessMessage(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Sender))
            {
                throw new ApplicationException("Message sender cannot be null or white space");
            }

            Message response = MessageFactory.CreateHelpMessage(message.Sender);
            await this.OutboundMessageQueue.AddMessage(response);

            return true;
        }
    }
}
