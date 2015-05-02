namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Different states for the <see cref="InspectAccountOperation"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InspectAccountOperationState
    {
        /// <summary>
        /// Checking if it's Sabbath for the account
        /// </summary>
        CheckingSabbath,

        /// <summary>
        /// Storing the Sabbath text
        /// </summary>
        StoringSabbathText,

        /// <summary>
        /// Archiving messages
        /// </summary>
        ArchivingMessages,
    }
}
