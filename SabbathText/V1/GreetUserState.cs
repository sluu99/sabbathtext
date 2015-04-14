namespace SabbathText.V1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Greet user operation states
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GreetUserState
    {
        /// <summary>
        /// Sending the message
        /// </summary>
        SendingMessage,

        /// <summary>
        /// Updating the account with the recently sent message
        /// </summary>
        UpdatingAccountContext,
    }
}
