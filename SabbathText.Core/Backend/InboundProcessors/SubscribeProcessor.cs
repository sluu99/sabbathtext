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
            await this.DataProvider.UpdateAccount(account);

            return MessageFactory.CreateSubscribedSuccessfully(message.Sender);
        }
    }
}
