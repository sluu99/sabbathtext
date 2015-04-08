namespace SabbathText.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This enumeration marks the conversation context the account has with the service.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConversationContext
    {
        /// <summary>
        /// No context
        /// </summary>
        NoContext,

        /// <summary>
        /// A greeting message is sent to the user
        /// </summary>
        Greetings,
    }
}
