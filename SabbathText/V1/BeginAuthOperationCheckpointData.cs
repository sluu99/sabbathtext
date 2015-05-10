namespace SabbathText.V1
{
    using System;

    /// <summary>
    /// Checkpoint data for <see cref="BeginAuthOperation"/>
    /// </summary>
    public class BeginAuthOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of the operation checkpoint data
        /// </summary>
        /// <param name="accountId">The account ID</param>
        public BeginAuthOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public GenericOperationState OperationState { get; set; }

        /// <summary>
        /// Gets or sets the incoming message
        /// </summary>
        public Message IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message
        /// </summary>
        public Message OutgoingMessage { get; set; }

        /// <summary>
        /// Gets or sets the authentication key
        /// </summary>
        public string AuthKey { get; set; }

        /// <summary>
        /// Gets or sets the incoming message ID
        /// </summary>
        public string IncomingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message ID
        /// </summary>
        public string OutgoingMessageId { get; set; }
    }
}
