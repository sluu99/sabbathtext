using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class ZipCodeUpdatedProcessor : AccountBasedProcessor
    {
        public ZipCodeUpdatedProcessor() : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override async Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account)
        {
            if (string.IsNullOrWhiteSpace(account.ZipCode))
            {
                throw new ApplicationException("Cannot process ZipCodeUpdated event with invalid account ZIP code");
            }

            // account data changed, just let the AccountCycle run
            await this.EventQueue.AddMessage(EventMessage.Create(account.PhoneNumber, EventType.AccountCycle, string.Empty));

            return null;
        }
    }
}
