using SabbathText.Core.Entities;
using System;
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

            // the Sabbath time could potentially change. Resetting the account cycle
            await this.ResetAccountCycle(account, TimeSpan.Zero);

            return null;
        }
    }
}
