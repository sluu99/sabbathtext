namespace SabbathText.V1
{
    using System;
    using SabbathText.Entities;

    /// <summary>
    /// This class represents a message
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the external ID
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the sender
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the recipient
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets the message body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the message template.
        /// </summary>
        public MessageTemplate Template { get; set; }
        
        /// <summary>
        /// Creates a new message to greet the user
        /// </summary>
        /// <param name="phoneNumber">The recipient phone number</param>
        /// <returns>The message</returns>
        public static Message CreateGreetingMessage(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.FreeForm,
                "Greetings from SabbathText.com! Text \"subscribe\" to get started. (Messaging rates may apply. Text \"STOP\" anytime)");
        }

        /// <summary>
        /// Creates a "not understandable" message.
        /// </summary>
        /// <param name="phoneNumber">The recipient phone number.</param>
        /// <returns>The message.</returns>
        public static Message CreateNotUnderstandable(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.NotUnderstandable,
                "Sorry, we didn't understand that. Can you try again?");
        }

        /// <summary>
        /// Creates a message to confirm the user subscription.
        /// </summary>
        /// <param name="phoneNumber">The recipient phone number.</param>
        /// <returns>The message.</returns>
        public static Message CreateSubscriptionConfirmed(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.SubscriptionConfirmed,
                "Thanks for subscribing! Just one more thing. Text us your ZIP code to calculate the sunset time.");
        }

        /// <summary>
        /// The user already subscribed, and has a ZIP code.
        /// </summary>
        /// <param name="phoneNumber">The account phone number.</param>
        /// <param name="zipCode">The ZIP code.</param>
        /// <returns>The message.</returns>
        public static Message CreateAlreadySubscribedWithZipCode(string phoneNumber, string zipCode)
        {
            string message =
                "You are already subscribed with the ZIP code {0}. You can change your ZIP code at anytime by texting \"Zip <zip>\"."
                .InvariantFormat(zipCode);

            return CreateMessage(
                phoneNumber,
                MessageTemplate.AlreadySubscribedWithZipCode,
                message);
        }

        /// <summary>
        /// Notifies the user that she needs to subscribe.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>The message.</returns>
        public static Message CreateSubscriptionRequired(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.SubscriptionRequired,
                "A subscription is required before we can proceed. Do you want to subscribe?");
        }

        /// <summary>
        /// Creates new message
        /// </summary>
        /// <param name="phoneNumber">The recipient</param>
        /// <param name="template">The message template.</param>
        /// <param name="body">The message body</param>
        /// <returns>The message</returns>
        private static Message CreateMessage(string phoneNumber, MessageTemplate template, string body)
        {
            return new Message
            {
                Body = body,
                Recipient = phoneNumber,
                Timestamp = Clock.UtcNow,
                Template = template,
            };
        }
        
        /// <summary>
        /// Converts this message to a <c>MessageEntity</c>.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="messageId">The message ID.</param>
        /// <param name="direction">The Message direction.</param>
        /// <param name="status">The message status.</param>
        /// <returns>The message entity.</returns>
        public MessageEntity ToEntity(string accountId, string messageId, MessageDirection direction, MessageStatus status)
        {
            return new MessageEntity
            {
                AccountId = accountId,
                Body = this.Body,
                Direction = direction,
                ExternalId = this.ExternalId,
                MessageId = messageId,
                MessageTimestamp = this.Timestamp,
                Recipient = this.Recipient,
                Sender = this.Sender,
                Status = status,
                Template = this.Template,
            };
        }
    }
}
