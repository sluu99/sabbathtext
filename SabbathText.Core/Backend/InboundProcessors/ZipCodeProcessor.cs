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
        public static readonly Regex ZipCodeRegex = new Regex(@"^Zip(?:Code)?\s*(?<ZipCode>\d+)$", RegexOptions.IgnoreCase);

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

                DateTime now = Clock.UtcNow;
                DateTime sabbathTime;

                Sabbath sabbath = new Sabbath()
                {
                    DataProvider = this.DataProvider,
                };

                DateTime lastSabbath = await sabbath.GetLastSabbath(location);
                
                // the current time has not passed the Sabbath message time
                if (now < lastSabbath + Sabbath.SabbathMessageGracePeriod)
                {
                    sabbathTime = lastSabbath;
                }
                else // we have passed the message sending time for the previous Sabbath
                {
                    sabbathTime = await sabbath.GetUpcomingSabbath(location);
                }
                
                await this.EventQueue.AddMessage(EventMessage.Create(message.Sender, EventType.ZipCodeUpdated, string.Empty));

                DateTime destinationSabbathTime = location.GetLocalTime(sabbathTime);

                return new MessageFactory().CreateConfirmZipCodeUpdate(message.Sender, location.ZipCode, location.City, location.State, destinationSabbathTime, sabbathTime - now);
            }
            else
            {
                return new MessageFactory().CreateCannotFindZipCode(account.PhoneNumber, zipCode);
            }
        }
    }
}
