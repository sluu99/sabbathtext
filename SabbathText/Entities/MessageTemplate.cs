namespace SabbathText.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of different template types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageTemplate
    {
        /// <summary>
        /// Free form message. The message does not have a predefined template.
        /// </summary>
        FreeForm,

        /// <summary>
        /// Greeting message.
        /// </summary>
        Greetings,

        /// <summary>
        /// Lets the user know that he/she has subscribed.
        /// </summary>
        SubscriptionConfirmed,

        /// <summary>
        /// The service could not understand the last incoming message.
        /// </summary>
        NotUnderstandable,

        /// <summary>
        /// The account is already subscribed with an existing ZIP code.
        /// </summary>
        AlreadySubscribedWithZipCode,

        /// <summary>
        /// Notify the user that he/she needs to subscribe before continuing.
        /// </summary>
        SubscriptionRequired,

        /// <summary>
        /// A message with instruction to update the ZIP code.
        /// </summary>
        UpdateZipInstruction,

        /// <summary>
        /// Cannot find a location for the ZIP code provided.
        /// </summary>
        LocationNotFound,

        /// <summary>
        /// The ZIP code is updated.
        /// </summary>
        ZipCodeUpdated,

        /// <summary>
        /// Sabbath text.
        /// </summary>
        SabbathText,

        /// <summary>
        /// Authentication key created.
        /// </summary>
        AuthKeyCreated,
    }
}
