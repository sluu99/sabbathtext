using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class SabbathProcessor : AccountBasedProcessor
    {
        static readonly TimeSpan SabbathMessageGap = TimeSpan.FromDays(5);

        public SabbathProcessor() : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            TimeSpan timeSinceLastSabbathMessage = Clock.UtcNow - account.LastSabbathMessageTime;

            if (timeSinceLastSabbathMessage < SabbathMessageGap)
            {
                Trace.TraceInformation("Time since last Sabbath message for account {0} is {1}. Skipped!", account.AccountId.Mask(4), timeSinceLastSabbathMessage);
            }

            account.LastSabbathMessageTime = Clock.UtcNow;

            return Task.FromResult(MessageFactory.CreateHappySabbath(account.PhoneNumber));
        }
    }
}
