namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Operation state for SubscribeMessageOperation.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscribeMessageOperationState
    {
        /// <summary>
        /// Processing the message.
        /// </summary>
        ProcessingMessage,

        /// <summary>
        /// Updating the account.
        /// </summary>
        UpdatingAccount,
    }
}
