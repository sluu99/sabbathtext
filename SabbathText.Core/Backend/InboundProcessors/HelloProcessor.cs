using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class HelloProcessor : AccountBasedProcessor
    {
        protected override Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account)
        {
            TemplatedMessage response = null;
            string recipient = message.Sender.Trim();

            if (account.Status == AccountStatus.Subscribed)
            {
                response = MessageFactory.CreateSubscriberGreetings(recipient);
            }
            else
            {
                response = MessageFactory.CreateGeneralGreetings(recipient);
            }

            return Task.FromResult(response);
        }
    }
}
