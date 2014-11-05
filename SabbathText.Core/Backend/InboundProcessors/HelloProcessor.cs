using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class HelloProcessor : IProcessor
    {
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
                Template = MessageTemplate.Greetings,
                CreateOn = Clock.UtcNow,
                Body = "Greetings from SabbathText.com!",
            });
        }
    }
}
