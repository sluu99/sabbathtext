namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Generic operation states used when a message is initiated from the server side.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceMessageOperationState
    {
        /// <summary>
        /// Sending the message
        /// </summary>
        SendingMessage,

        /// <summary>
        /// Updating the account with the recently sent message
        /// </summary>
        UpdatingAccount,
    }
}
