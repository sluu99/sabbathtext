using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class SabbathProcessor : AccountBasedProcessor
    {
        static readonly TimeSpan SabbathMessageGap = TimeSpan.FromDays(5);
        

        public SabbathProcessor()
            : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected async override Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            // IMPORTANT!!
            // Sabbath processor is retired. Nobody should be scheduling this event.
            // The AccountCycleProcessor is now responsible for sending out Sabbath messages

            // for legacy support, reschedule an AccountCycle event when an account hits its Sabbath event
            account.CycleKey = Guid.NewGuid().ToString();
            await this.EventQueue.AddMessage(EventMessage.Create(account.PhoneNumber, EventType.AccountCycle, account.CycleKey));

            // update the cycle key last, so that if it fails ,the retry of the current message will have the matching cycle key
            await this.DataProvider.UpdateAccount(account);

            Trace.TraceInformation("Sabbath event discarded for account {0}", account.AccountId);
            return null;
        }
    }
}
