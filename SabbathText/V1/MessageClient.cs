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
        /// Initializes the message manager to send messages using Twilio
        /// </summary>
        /// <param name="twilioAccount">The Twilio account</param>
        /// <param name="twilioToken">The Twilio access token</param>
        /// <param name="twilioPhoneNumber">The Twilio phone number</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Twilio is a company name.")]
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
