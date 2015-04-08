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
        /// Greeting message
        /// </summary>
        Greetings,
    }
}
