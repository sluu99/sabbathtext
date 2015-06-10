namespace SabbathText.V1
{
    /// <summary>
    /// Checkpoint data for <c>SubscribeMessageOperation</c>
    /// </summary>
    public class SubscribeMessageOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public SubscribeMessageOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation state.
        /// </summary>
        public RespondingMessageOperationState State { get; set; }

        /// <summary>
        /// Gets or sets the incoming message.
        /// </summary>
        public Message IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message.
        /// </summary>
        public Message OutgoingMessage { get; set; }

        /// <summary>
        /// Gets or sets the incoming message ID.
        /// </summary>
        public string IncomingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message ID.
        /// </summary>
        public string OutgoingMessageId { get; set; }
    }
}
