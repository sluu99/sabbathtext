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
        public static readonly TimeSpan ShortResetCycleDelay = TimeSpan.FromSeconds(15);

        public AccountCycleProcessor()
            : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {   
            Location location = await this.DataProvider.GetLocationByZipCode(account.ZipCode);            

            TimeSpan timeUntilNextCycle = Account.CycleDuration;
            
            Tuple<CustomMessageSchedule, TimeSpan> customScheduleResult = await this.GetActionableCustomSchedule(account, location);
            if (customScheduleResult.Item1 != null)
            {
                Trace.TraceInformation("Sending out the custom message {0} for account {1}", customScheduleResult.Item1.ScheduleId, account.AccountId);

                await this.ResetAccountCycle(account, ShortResetCycleDelay);
                await this.DataProvider.CreateAccountCustomMessage(account.AccountId, customScheduleResult.Item1.ScheduleId);

                return new MessageFactory().CreateCustomMessage(account.PhoneNumber, customScheduleResult.Item1.Body);
            }

            if (customScheduleResult.Item2 < timeUntilNextCycle)
            {
                timeUntilNextCycle = customScheduleResult.Item2;
            }

            Tuple<bool, TimeSpan> sabbathTextResult = await this.ShouldSendSabbathMessage(account, location);
            if (sabbathTextResult.Item1 == true)
            {
                Trace.TraceInformation("Sending Sabbath text to account {0}", account.AccountId);

                await this.ResetAccountCycle(account, ShortResetCycleDelay);
                
                account.LastSabbathMessageTime = Clock.UtcNow;
                await this.DataProvider.UpdateAccount(account);

                return new MessageFactory().CreateHappySabbath(account.PhoneNumber);
            }

            if (sabbathTextResult.Item2 < timeUntilNextCycle)
            {
                timeUntilNextCycle = sabbathTextResult.Item2;
            }

            string parameters = message.Body.GetParameters();
            if (!string.IsNullOrWhiteSpace(parameters) && string.Equals(parameters, account.CycleKey))
            {
                await this.ResetAccountCycle(account, timeUntilNextCycle);
            }

            return null;
        }
        
        private async Task<Tuple<bool, TimeSpan>> ShouldSendSabbathMessage(Account account, Location location)
        {
            if (location == null || string.IsNullOrWhiteSpace(account.ZipCode))
            {
                return new Tuple<bool, TimeSpan>(false, Account.CycleDuration);
            }

            bool shouldSendSabbathText = false;

            Sabbath sabbath = new Sabbath
            {
                DataProvider = this.DataProvider
            };

            // more than X days ago since we sent a Sabbath message (this could also mean that we never sent a Sabbath message)
            if (Clock.UtcNow - account.LastSabbathMessageTime > Sabbath.SabbathMessageGap)
            {
                DateTime lastSabbath = await sabbath.GetLastSabbath(location);

                // we should still send a message for the last Sabbath
                // this will also handle the case where the queue is backed up and the Cycle event wakes up after Sabbath already started
                DateTime sabbathMessageEndTime = lastSabbath + Sabbath.SabbathMessageGracePeriod;
                if (Clock.UtcNow < sabbathMessageEndTime)
                {
                    shouldSendSabbathText = true;
                }
            }

            TimeSpan timeUntilNextCycle = Account.CycleDuration;
            DateTime upcomingSabbath = await sabbath.GetUpcomingSabbath(location);

            // check if Sabbath comes before the next duration                
            if (upcomingSabbath < Clock.UtcNow + Account.CycleDuration)
            {
                timeUntilNextCycle = upcomingSabbath - Clock.UtcNow;
            }

            return new Tuple<bool, TimeSpan>(shouldSendSabbathText, timeUntilNextCycle);
        }


        private async Task<Tuple<CustomMessageSchedule, TimeSpan>> GetActionableCustomSchedule(Account account, Location location)
        {
            if (location == null || string.IsNullOrWhiteSpace(account.ZipCode))
            {
                return new Tuple<CustomMessageSchedule,TimeSpan>(null, Account.CycleDuration);
            }

            DateTime destTime = location.GetLocalTime(Clock.UtcNow);
            IEnumerable<CustomMessageSchedule> schedules = await this.DataProvider.GetCustomMessageSchedules(destTime.Date, Account.CycleDuration);
            IEnumerable<AccountCustomMessage> sentMessages = await this.DataProvider.GetAccountCustomMessages(account.AccountId);

            CustomMessageSchedule[] unsentSchedules = (
                from s in schedules
                where !sentMessages.Any(sent => sent.ScheduleId == s.ScheduleId)
                orderby s.ScheduleDate
                select s
            ).ToArray();

            Trace.TraceInformation("Found {0} unsent schedules", unsentSchedules.Length);

            DateTime[] scheduleTimes = new DateTime[unsentSchedules.Length];

            CustomMessageSchedule actionableSchedule = null;            
                        
            // calculate schedule times and look for an actionable schedule along the way
            for (int i = 0; i < unsentSchedules.Length; i++ )
            {                
                CustomMessageSchedule schedule = unsentSchedules[i];

                Trace.TraceInformation("Schedule {0}", schedule.ScheduleId);

                LocationTimeInfo timeInfo = await this.DataProvider.GetTimeInfoByZipCode(account.ZipCode, schedule.ScheduleDate);

                if (schedule.OffsetFrom == TimeOffsetType.Sunrise)
                {
                    scheduleTimes[i] = timeInfo.Sunrise + TimeSpan.FromSeconds(schedule.SecondsOffset);
                }
                else
                {
                    scheduleTimes[i] = timeInfo.Sunset + TimeSpan.FromSeconds(schedule.SecondsOffset);
                }

                Trace.TraceInformation("Schedule time: {0}", scheduleTimes[i]);

                if (actionableSchedule == null && scheduleTimes[i] <= Clock.UtcNow && Clock.UtcNow <= scheduleTimes[i] + TimeSpan.FromSeconds(schedule.GracePeriod))
                {
                    actionableSchedule = schedule;                    
                }
            }
            
            // the convention is that if it is actionable then we start always return Timespan.Zero
            if (actionableSchedule != null)
            {
                return new Tuple<CustomMessageSchedule, TimeSpan>(actionableSchedule, TimeSpan.Zero);
            }

            // look for a next schedule date
            DateTime nextSchedule = scheduleTimes.FirstOrDefault(x => x >= Clock.UtcNow);
            if (nextSchedule != DateTime.MinValue)
            {
                return new Tuple<CustomMessageSchedule, TimeSpan>(null, nextSchedule - Clock.UtcNow);
            }

            return new Tuple<CustomMessageSchedule, TimeSpan>(null, Account.CycleDuration);
        }
    }
}
