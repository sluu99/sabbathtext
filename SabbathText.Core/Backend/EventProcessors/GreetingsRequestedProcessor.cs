using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class GreetingsRequestedProcessor : AccountBasedProcessor
    {
        public GreetingsRequestedProcessor() : base(subscriberRequired: false, skipRecordMessage: true)
        {
        }

        protected override Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account)
        {
            TemplatedMessage response = null;

            if (account.Status == AccountStatus.Subscribed)
            {
                response = new MessageFactory().CreateSubscriberGreetings(account.PhoneNumber);
            }
            else
            {
                response = new MessageFactory().CreateGeneralGreetings(account.PhoneNumber);
            }

            return Task.FromResult(response);
        }
    }
}
