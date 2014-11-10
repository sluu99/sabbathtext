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

        public AccountCycleProcessor() : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            if (account.Status != AccountStatus.Subscribed)
            {
                return null;
            }

            string parameters = message.Body.GetParameters();

            if (!string.IsNullOrWhiteSpace(parameters) && string.Equals(parameters, account.CycleKey))
            {
                await Reschedule(account);
            }

            await CheckForSabbath(account);
            

            return null;
        }

        private async Task CheckForSabbath(Account account)
        {
            if (string.IsNullOrWhiteSpace(account.ZipCode))
            {
                return;
            }

            Location location = await this.DataProvider.GetLocationByZipCode(account.ZipCode);

            if (location == null)
            {
                throw new ApplicationException(string.Format("Cannot find location for ZIP code {0}", account.ZipCode));
            }

            DateTime locationNextSabbath = await new Sabbath().GetUpcomingSabbath(location, TimeSpan.Zero);

            DateTime utcNextSabbath = locationNextSabbath.AddHours(-1 * location.TimeZoneOffset);
                        
            // will the next cycle potentially miss the upcoming Sabbath?
            // assuming that the next cycle could potentially wake up at 1.5 cycles away
            DateTime nextCycle = Clock.UtcNow.AddSeconds(Account.CycleDuration.TotalSeconds * 1.5);

            if (nextCycle >= utcNextSabbath)
            {
                TimeSpan timeUntilSabbath = utcNextSabbath - Clock.UtcNow;

                // queue up a Sabbath event
                await this.EventQueue.AddMessage(
                    EventMessage.Create(account.PhoneNumber, EventType.Sabbath, string.Empty),
                    timeUntilSabbath
                );

                Trace.TraceInformation("Sabbath event for account {0} will be visible in {1}", account.AccountId.Mask(4), timeUntilSabbath);
            }
        }

        private async Task Reschedule(Account account)
        {
            account.CycleKey = Guid.NewGuid().ToString();
            
            await this.EventQueue.AddMessage(EventMessage.Create(account.PhoneNumber, EventType.AccountCycle, account.CycleKey), Account.CycleDuration);

            // update the cycle key last, so that if it fails ,the retry of the current message will have the matching cycle key
            await this.DataProvider.UpdateAccount(account);

            Trace.TraceInformation("Next cycle scheduled on {0}", Clock.UtcNow + Account.CycleDuration);
        }
    }
}
