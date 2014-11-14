using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class AccountCycleProcessor : AccountBasedProcessor
    {
        
        public static readonly Regex AccountCycleRegex = new Regex(@"^AccountCycle(?:\s(?<CycleKey>.+))?$", RegexOptions.IgnoreCase);

        public AccountCycleProcessor()
            : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {            
            DateTime now = Clock.UtcNow;
            Location location = await this.DataProvider.GetLocationByZipCode(account.ZipCode);
            Sabbath sabbath = new Sabbath
            {
                DataProvider = this.DataProvider,
            };
            
            string parameters = message.Body.GetParameters();

            if (!string.IsNullOrWhiteSpace(parameters) && string.Equals(parameters, account.CycleKey))
            {
                await Reschedule(account, location);
            }

            if (!string.IsNullOrWhiteSpace(account.ZipCode))
            {
                // more than X days ago since we sent a Sabbath message (this could also mean that we never sent a Sabbath message)
                if (now - account.LastSabbathMessageTime > Sabbath.SabbathMessageGap)
                {                    
                    DateTime lastSabbath = await sabbath.GetLastSabbath(location);

                    // we should still send a message for the last Sabbath
                    // this will also handle the case where the queue is backed up and the Cycle event wakes up after Sabbath already started
                    DateTime sabbathMessageEndTime = lastSabbath + Sabbath.SabbathMessageTimeSpan;
                    if (now < sabbathMessageEndTime)
                    {
                        account.LastSabbathMessageTime = Clock.UtcNow;

                        await this.DataProvider.UpdateAccount(account);

                        Trace.TraceInformation("Sending a Sabbath message to account {0}.", account.AccountId);

                        return new MessageFactory().CreateHappySabbath(account.PhoneNumber);
                    }
                }
            }
            
            return null;
        }
        
        private async Task Reschedule(Account account, Location location)
        {
            TimeSpan timeUntilNextCycle = Account.CycleDuration;
            DateTime now = Clock.UtcNow;

            if (location != null)
            {
                Sabbath sabbath = new Sabbath
                {
                    DataProvider = this.DataProvider,
                };

                DateTime upcomingSabbath = await sabbath.GetUpcomingSabbath(location);

                // check if Sabbath comes before the next duration                
                if (upcomingSabbath < now + Account.CycleDuration)
                {
                    timeUntilNextCycle = upcomingSabbath - now;
                }
            }

            await this.ResetAccountCycle(account, timeUntilNextCycle);

            Trace.TraceInformation("Next cycle scheduled on {0} for account {1}", now + timeUntilNextCycle, account.AccountId);
        }
    }
}
