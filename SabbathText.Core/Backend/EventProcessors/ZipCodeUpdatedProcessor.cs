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

            Location location = await this.DataProvider.GetLocationByZipCode(account.ZipCode);

            if (location == null)
            {
                throw new ApplicationException(string.Format("Cannot find location for ZIP code {0}", account.ZipCode));
            }

            DateTime locationNextSabbath = Sabbath.GetLocationNextSabbath(location.Latitude, location.Longitude, location.TimeZoneOffset);
            DateTime utcNextSabbath = locationNextSabbath.AddHours(-1 * location.TimeZoneOffset);            
            TimeSpan timeUntilSabbath = utcNextSabbath - Clock.UtcNow;

            // this is only possible if the Sabbath calculation math messes up
            if (timeUntilSabbath > MessageQueue.MaxVisiblityDelay)
            {
                // let's queue this event to try again later
                await this.EventQueue.AddMessage(
                    EventMessage.Create(account.AccountId, EventType.ZipCodeUpdated, string.Empty), 
                    TimeSpan.FromSeconds(MessageQueue.MaxVisiblityDelay.TotalSeconds / 2)
                );
            }
            else
            {
                // queue up a Sabbath event
                await this.EventQueue.AddMessage(
                    EventMessage.Create(account.AccountId, EventType.Sabbath, string.Empty),
                    timeUntilSabbath
                );

                Trace.TraceInformation("Sabbath event for account {0} will be visible in {1}", account.AccountId.Mask(4), timeUntilSabbath);
            }

            return null;
        }
    }
}
