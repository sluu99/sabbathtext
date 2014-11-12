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

        protected override Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            Trace.TraceInformation("Sabbath event discarded for account {0}", account.AccountId);
            return Task.FromResult<TemplatedMessage>(null);
        }
    }
}
