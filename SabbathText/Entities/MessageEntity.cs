namespace SabbathText.Entities
{
    using System;
    using KeyValueStorage;

    /// <summary>
    /// A message entity
    /// </summary>
    public class MessageEntity : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the account ID this message is associated with
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the message ID
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the message sender
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the message recipient
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets the message body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the message timestamp
        /// </summary>
        public DateTime MessageTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the message direction
        /// </summary>
        public MessageDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the message status
        /// </summary>
        public MessageStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the message template.
        /// </summary>
        public MessageTemplate Template { get; set; }
    }
}
