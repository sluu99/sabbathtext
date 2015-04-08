namespace SabbathText.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

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
}
