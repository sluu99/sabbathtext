namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Generic operation states used when responding to an incoming message
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RespondingMessageOperationState
    {
        /// <summary>
        /// Processing message.
        /// </summary>
        ProcessingMessage,

        /// <summary>
        /// Sending response to the user,
        /// </summary>
        SendingResponse,

        /// <summary>
        /// Updating account.
        /// </summary>
        UpdatingAccount,
    }
}
