namespace SabbathText.Entities
{
    using System;
    using KeyValueStorage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

    /// <summary>
    /// The message direction
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageDirection
    {
        /// <summary>
        /// An incoming message
        /// </summary>
        Incoming,

        /// <summary>
        /// An outgoing message
        /// </summary>
        Outgoing
    }

    /// <summary>
    /// The message status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageStatus
    {
        /// <summary>
        /// An incoming message is received
        /// </summary>
        Received,

        /// <summary>
        /// An incoming message is responded
        /// </summary>
        Responded,

        /// <summary>
        /// An outgoing message is queued
        /// </summary>
        Queued,

        /// <summary>
        /// An outgoing message is sent
        /// </summary>
        Sent,
    }

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
    }
}
