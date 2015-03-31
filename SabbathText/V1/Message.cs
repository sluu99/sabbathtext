namespace SabbathText.V1
{
    using System;

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
        /// Creates a new message to greet the user
        /// </summary>
        /// <param name="phoneNumber">The recipient phone number</param>
        /// <returns>The message</returns>
        public static Message CreateGreetingMessage(string phoneNumber)
        {
            return CreateMessage(
                phoneNumber,
                "Greetings from SabbathText.com! Text \"subscribe\" to get started. (Messaging rates may apply. Text \"STOP\" anytime)");
        }

        /// <summary>
        /// Creates new message
        /// </summary>
        /// <param name="phoneNumber">The recipient</param>
        /// <param name="body">The message body</param>
        /// <returns>The message</returns>
        private static Message CreateMessage(string phoneNumber, string body)
        {
            return new Message
            {
                Body = body,
                Recipient = phoneNumber,
                Timestamp = Clock.UtcNow,
            };
        }
    }
}
