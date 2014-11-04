using SabbathText.Core.Entities;
using System;

namespace SabbathText.Core
{
    public class MessageFactory
    {
        public static Message CreateGreetingsMessage(string recipient)
        {
            return new TemplatedMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Recipient = recipient,
                Template = MessageTemplate.Greetings,
                CreateOn = Clock.UtcNow,
                Body = "Greetings from SabbathText.com!",
            };
        }

        public static Message CreateHelpMessage(string recipient)
        {
            return new TemplatedMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Recipient = recipient,
                Template = MessageTemplate.Help,
                CreateOn = Clock.UtcNow,
                Body = "Go to http://www.SabbathText.com for more information, or text \"stop\" to opt out from receiving more messages.",
            };
        }
    }
}
