using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class ZipCodeProcessor : AccountBasedProcessor
    {
        public ZipCodeProcessor() : base(subscriberRequired: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            /*
             * The accepted formats for the body are
             * 
             * ZIP 123456
             * ZIPCODE 123456
             * 
             */
            string body = message.Body;

            string[] parts = body.Split(' ');

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return MessageFactory.CreateBadRequest(message.Sender, "Cannot update ZIP code");
            }

            string zipCode = parts[1].Trim();

            Location location = await this.DataProvider.GetLocationByZipCode(zipCode);

            if (location != null)
            {
                account.ZipCode = location.ZipCode;
                await this.DataProvider.UpdateAccount(account);

                DateTime sabbath = Sabbath.GetLocationNextSabbath(location.Latitude, location.Longitude, location.TimeZoneOffset);

                await this.EventQueue.AddMessage(EventMessage.Create(message.Sender, EventType.ZipCodeUpdated, string.Empty));

                return MessageFactory.CreateConfirmZipCodeUpdate(message.Sender, location.ZipCode, location.LocationName, sabbath);
            }
            else
            {
                return MessageFactory.CreateBadRequest(message.Sender, string.Format("Cannot find your location {0}", zipCode));
            }
        }
    }
}
