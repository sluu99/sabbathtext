using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class SubscribeProcessor : AccountBasedProcessor
    {
        protected override async Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account)
        {
            string oldStatus = account.Status;
            account.Status = AccountStatus.Subscribed;

            await this.ResetAccountCycle(account, TimeSpan.Zero);            

            if (account.Status != oldStatus)
            {                
                await this.EventQueue.AddMessage(EventMessage.Create(account.PhoneNumber, EventType.AccountSubscribed, string.Empty));
            }            
            
            if (string.IsNullOrWhiteSpace(account.ZipCode))
            {
                return new MessageFactory().CreateSubscribedMissingZipCode(message.Sender);
            }
            else
            {
                return new MessageFactory().CreateSubscribedConfirmZipCode(message.Sender, account.ZipCode);
            }
        }
    }
}
