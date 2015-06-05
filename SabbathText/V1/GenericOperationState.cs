namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Generic states of an operation.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GenericOperationState
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
