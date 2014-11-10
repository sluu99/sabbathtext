using NodaTime;
using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class ZipCodeProcessor : AccountBasedProcessor
    {
        public static readonly Regex ZipCodeRegex = new Regex(@"^Zip(?:Code)?\s*(?<ZipCode>\d*)$", RegexOptions.IgnoreCase);
        static readonly TimeSpan SunsetNotWithin = TimeSpan.FromMinutes(5);

        public ZipCodeProcessor() : base(subscriberRequired: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            /*
             * The accepted formats for the body are
             * 
             * Zip 12345
             * ZipCode 12345
             * Zip12345
             * ZipCode12345
             * 
             */
            string body = message.Body.ExtractAlphaNumericSpace().Trim();

            Match match = ZipCodeRegex.Match(body);
            string zipCode = match.Groups["ZipCode"].Value;

            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return new MessageFactory().CreateBadRequest(message.Sender, "Please provide a ZIP code!");
            }
            
            Location location = await this.DataProvider.GetLocationByZipCode(zipCode);

            if (location != null)
            {
                account.ZipCode = location.ZipCode;

                await this.DataProvider.UpdateAccount(account);

                // if Sabbath starts within 5 minutes (SunsetNotWithin), we do not want to use that
                // since the queue might have some delay and we might not actually deliver the Sabbath message on time
                DateTime sabbath = await new Sabbath().GetUpcomingSabbath(location, SunsetNotWithin);

                await this.EventQueue.AddMessage(EventMessage.Create(message.Sender, EventType.ZipCodeUpdated, string.Empty));

                DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[location.TimeZoneName];                
                DateTime destinationSabbath = Instant.FromDateTimeUtc(sabbath).InZone(timeZone).ToDateTimeUnspecified();

                return new MessageFactory().CreateConfirmZipCodeUpdate(message.Sender, location.ZipCode, location.City, location.State, destinationSabbath);
            }
            else
            {
                return new MessageFactory().CreateCannotFindZipCode(account.PhoneNumber, zipCode);
            }
        }
    }
}
