using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class SubscribeProcessor : AccountBasedProcessor
    {
        protected override async Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account)
        {
            account.Status = AccountStatus.Subscribed;

            // create a new cycle key to make sure the other cycle events won't try to reschedule themselves
            account.CycleKey = Guid.NewGuid().ToString(); 

            await this.DataProvider.UpdateAccount(account);

            await this.EventQueue.AddMessage(EventMessage.Create(account.AccountId, EventType.AccountCycle, account.CycleKey));
            
            if (string.IsNullOrWhiteSpace(account.ZipCode))
            {
                return MessageFactory.CreateSubscribedMissingZipCode(message.Sender);
            }
            else
            {
                return MessageFactory.CreateSubscribedConfirmZipCode(message.Sender, account.ZipCode);
            }
        }
    }
}
