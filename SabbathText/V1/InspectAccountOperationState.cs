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
        /// Checking for announcements to be sent out
        /// </summary>
        CheckingAnnouncements,

        /// <summary>
        /// Storing the announcement text
        /// </summary>
        StoringAnnouncementText,

        /// <summary>
        /// Archiving messages
        /// </summary>
        ArchivingMessages,
    }
}
