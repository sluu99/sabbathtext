namespace SabbathText.V1
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using SabbathText.Entities;
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
        /// <param name="trackingId">The tracking ID used for this message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Whether the message was sent</returns>
        public virtual async Task<bool> SendMessage(Message message, string trackingId, CancellationToken cancellationToken)
        {
            GoodieBag bag = GoodieBag.Create();

            if (trackingId != null)
            {
                try
                {
                    await bag.TrackerStore.Insert(
                        new TrackerEntity
                        {
                            TrackingId = trackingId,
                        },
                        cancellationToken);
                }
                catch (DuplicateKeyException)
                {
                    bag.TelemetryTracker.MessageSkipped(message.Template, trackingId);
                    return false;
                }
            }

            TwilioRestClient client = new TwilioRestClient(this.twilioAccount, this.twilioToken);
            Twilio.Message twilioMessage = client.SendMessage(this.twilioPhoneNumber, message.Recipient, message.Body);
            message.ExternalId = twilioMessage.Sid;

            bag.TelemetryTracker.MessageSent(message.Template, message.Body, trackingId);

            return true;
        }
    }
}
