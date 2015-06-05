namespace SabbathText.V1
{
    /// <summary>
    /// Different types of message client
    /// </summary>
    public enum MessageClientType
    {
        /// <summary>
        /// In-memory message client
        /// </summary>
        InMemory,

        /// <summary>
        /// <c>Twilio</c> message client
        /// </summary>
        Twilio
    }
}
