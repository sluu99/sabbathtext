using System;
using System.Threading.Tasks;
using Twilio;

namespace SabbathText.Core
{
    public class TwilioMessageSender : IMessageSender
    {
        private string twilioAccount = null;
        private string twilioToken = null;
        private string twilioPhoneNumber = null;

        public TwilioMessageSender()
        {
            this.twilioAccount = Environment.GetEnvironmentVariable("ST_TWILIO_ACCOUNT");
            this.twilioToken = Environment.GetEnvironmentVariable("ST_TWILIO_TOKEN");
            this.twilioPhoneNumber = Environment.GetEnvironmentVariable("ST_TWILIO_PHONE_NUMBER");
        }
       
        public Task<string> Send(Entities.Message message)
        {
            return Task.Run(() =>
            {
                TwilioRestClient client = new TwilioRestClient(this.twilioAccount, this.twilioToken);
                Message twilioMessage = client.SendMessage(this.twilioPhoneNumber, message.Recipient, message.Body);

                return twilioMessage.Sid;
            });
        }
    }
}
