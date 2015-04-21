namespace SabbathText.V1
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Twilio;

    /// <summary>
    /// This class manages messages
    /// </summary>
    public class MessageClient
    {
        private string twilioAccount;
        private string twilioToken;
        private string twilioPhoneNumber;

        /// <summary>
        /// Initializes the message manager to send messages using <c>Twilio</c>.
        /// </summary>
        /// <param name="twilioAccount">The <c>Twilio</c> account</param>
        /// <param name="twilioToken">The <c>Twilio</c> access token</param>
        /// <param name="twilioPhoneNumber">The <c>Twilio</c> phone number</param>
        public void InitTwilio(string twilioAccount, string twilioToken, string twilioPhoneNumber)
        {
            this.twilioAccount = twilioAccount;
            this.twilioToken = twilioToken;
            this.twilioPhoneNumber = twilioPhoneNumber;
        }

        /// <summary>
        /// Sends a message and sets its external ID
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>The operation task</returns>
        public virtual Task SendMessage(Message message)
        {
            return Task.Run(() =>
            {
                TwilioRestClient client = new TwilioRestClient(this.twilioAccount, this.twilioToken);
                Twilio.Message twilioMessage = client.SendMessage(this.twilioPhoneNumber, message.Recipient, message.Body);
                message.ExternalId = twilioMessage.Sid;
            });
        }
    }
}
