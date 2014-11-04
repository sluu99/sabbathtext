using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class HelpProcessor : IProcessor
    {
        public HelpProcessor()
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

            Message response = MessageFactory.CreateHelpMessage(message.Sender);
            await this.MessageQueue.QueueOutboundMessage(response);

            return true;
        }
    }
}
