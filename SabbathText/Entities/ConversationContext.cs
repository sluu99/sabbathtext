namespace SabbathText.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This enumeration marks the conversation context the account has with the service (based on the last sent message).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConversationContext
    {
        /// <summary>
        /// No context
        /// </summary>
        Unknown,

        /// <summary>
        /// A greeting message is sent to the user
        /// </summary>
        Greetings,

        /// <summary>
        /// Confirms that the user has subscribed.
        /// </summary>
        SubscriptionConfirmed,

        /// <summary>
        /// The account is already subscribed with an existing ZIP code.
        /// </summary>
        AlreadySubscribedWithZipCode,
    }
}
