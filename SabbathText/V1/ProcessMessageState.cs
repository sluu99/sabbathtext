namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Process message operation state
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProcessMessageState
    {
        /// <summary>
        /// The message is being processed
        /// </summary>
        Processing,

        /// <summary>
        /// Sending the outgoing message
        /// </summary>
        SendingReply,

        /// <summary>
        /// Updating the account conversation context
        /// </summary>
        UpdatingAccount,

        /// <summary>
        /// Update the incoming and outgoing message status
        /// </summary>
        UpdatingMessageStatus,
    }
}
