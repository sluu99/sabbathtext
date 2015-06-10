namespace SabbathText.V1
{
    using System;
    using SabbathText.Entities;
using SabbathText.Location.V1;

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
        /// Creates a message to prompt the user for the ZIP code.
        /// </summary>
        /// <param name="phoneNumber">The recipient phone number.</param>
        /// <returns>The message.</returns>
        public static Message CreatePromptZipCode(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.PromptZipCode,
                "Just one more thing! Text us your ZIP code to calculate the sunset time. For example: \"Zip 12345\".");
        }

        /// <summary>
        /// The user subscribed, and has a ZIP code.
        /// </summary>
        /// <param name="phoneNumber">The account phone number.</param>
        /// <param name="location">The location.</param>
        /// <returns>The message.</returns>
        public static Message CreateSubscribedForLocation(string phoneNumber, LocationInfo location)
        {
            string message = "You are subscribed to Sabbath texts for {0}, {1}.".InvariantFormat(location.PrimaryCity, location.State);
            if (location.IsSabbath() == false)
            {
                message += " Expect a text from us when Sabbath starts!";
            }

            message += " (You can change your ZIP code at anytime by texting \"Zip <zip>\")";

            return CreateMessage(
                phoneNumber,
                MessageTemplate.SubscribedForZipCode,
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
                "A subscription is required before we can proceed. Do you want to subscribe? (Text \"subscribe\")");
        }

        /// <summary>
        /// Sends the user the instruction for updating ZIP code.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>The message.</returns>
        public static Message CreateUpdateZipInstruction(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.UpdateZipInstruction,
                "To update your ZIP code, text the word \"zip\" followed by your ZIP code. For example: \"Zip 12345\".");
        }

        /// <summary>
        /// Notifies the user that we could not find a location for the ZIP code provided.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="badZipCode">The Zip code.</param>
        /// <returns>The message.</returns>
        public static Message CreateLocationNotFound(string phoneNumber, string badZipCode)
        {
            return CreateMessage(
                phoneNumber,
                MessageTemplate.LocationNotFound,
                "Sorry, but we cannot find a location for the ZIP code {0}.".InvariantFormat(badZipCode));
        }

        /// <summary>
        /// Creates a new text to send out during the Sabbath.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="verseNumber">The Bible verse number.</param>
        /// <param name="verseContent">The Bible verse content.</param>
        /// <returns>The message.</returns>
        public static Message CreateSabbathText(string phoneNumber, string verseNumber, string verseContent)
        {
            string body = "Happy Sabbath!\r\n\"{0}\" -- {1}".InvariantFormat(verseContent, verseNumber);
            
            return CreateMessage(
                phoneNumber,
                MessageTemplate.SabbathText,
                body);
        }

        /// <summary>
        /// Creates a new text to send out a Bible verse
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="verseNumber">The Bible verse number.</param>
        /// <param name="verseContent">The Bible verse content.</param>
        /// <returns>The message.</returns>
        public static Message CreateBibleVerse(string phoneNumber, string verseNumber, string verseContent)
        {
            string body = "{0}\r\n-- {1}".InvariantFormat(verseContent, verseNumber);

            return CreateMessage(
                phoneNumber,
                MessageTemplate.BibleVerse,
                body);
        }

        /// <summary>
        /// Creates a message with authentication key information.
        /// </summary>
        /// <param name="phoneNumber">The account phone number.</param>
        /// <param name="authKey">The authentication key.</param>
        /// <param name="authKeyExpirationMinutes">The number of minutes until the authentication key expires.</param>
        /// <returns>The message.</returns>
        public static Message CreateAuthKeyCreated(string phoneNumber, string authKey, int authKeyExpirationMinutes)
        {
            string body = "Your authentication key is {0}. It will expire in approximately {1} minutes.".InvariantFormat(
                authKey,
                authKeyExpirationMinutes);

            return CreateMessage(
                phoneNumber,
                MessageTemplate.AuthKeyCreated,
                body);
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
