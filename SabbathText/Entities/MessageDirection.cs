namespace SabbathText.Entities
{
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
        Outgoing,
    }
}
