namespace SabbathText.V1
{
    using SabbathText.Entities;

    /// <summary>
    /// Captures the data used for <see cref="HelloMessageOperation"/>
    /// </summary>
    public class HelloMessageOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="accountId">The account ID associated with the operation.</param>
        public HelloMessageOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the ID of the outgoing message.
        /// </summary>
        public string OutgoingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the incoming message.
        /// </summary>
        public string IncomingMessageId { get; set; }
        
        /// <summary>
        /// Gets or sets the incoming message.
        /// </summary>
        public Message IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message.
        /// </summary>
        public Message OutgoingMessage { get; set; }

        /// <summary>
        /// Gets or sets the account status.
        /// This is cached in the operation data so that the operation can be idempotent,
        /// even if the account status has changed.
        /// </summary>
        public AccountStatus AccountStatus { get; set; }

        /// <summary>
        /// Gets or sets the operation state.
        /// </summary>
        public RespondingMessageOperationState OperationState { get; set; }
    }
}
