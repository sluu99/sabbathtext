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

                DateTime sabbath = Sabbath.GetLocationNextSabbath(location.Latitude, location.Longitude, location.TimeZoneOffset);

                await this.EventQueue.AddMessage(EventMessage.Create(message.Sender, EventType.ZipCodeUpdated, string.Empty));

                return new MessageFactory().CreateConfirmZipCodeUpdate(message.Sender, location.ZipCode, location.LocationName, sabbath);
            }
            else
            {
                return new MessageFactory().CreateCannotFindZipCode(account.PhoneNumber, zipCode);
            }
        }
    }
}
